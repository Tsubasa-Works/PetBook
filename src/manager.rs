use crate::typing::*;
use futures::future::join_all;
use futures::StreamExt;
use json::JsonValue;
use reqwest::{header, Client};
use rusqlite::{Connection, OpenFlags, Params, Row, Transaction};
use std::cmp::Ordering::{Equal, Greater, Less};
use std::collections::HashMap;
use std::ffi::CString;
use std::path::PathBuf;
use tokio::io::AsyncWriteExt;
use tokio::runtime;

pub type Result<T> = std::result::Result<T, Box<dyn std::error::Error + Send + Sync>>;

struct NetworkManager {
    opener: Client,
    rt: runtime::Runtime,
}

impl NetworkManager {
    pub fn new() -> Self {
        let mut headers = header::HeaderMap::new();
        headers.insert(
            header::USER_AGENT,
            header::HeaderValue::from_str("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36").unwrap(),
        );
        let opener = reqwest::Client::builder()
            .default_headers(headers)
            .build()
            .unwrap();
        let rt = runtime::Builder::new_multi_thread()
            .enable_all()
            .build()
            .unwrap();
        Self { opener, rt }
    }
    pub async fn get_single(&self, src: String, dst: PathBuf) -> Result<()> {
        for try_time in 0..3 {
            let res = {
                let res = self.opener.get(&src).send().await?;
                if res.status().is_success() {
                    let res = res.bytes().await?.to_vec();
                    let mut writer =
                        tokio::io::BufWriter::new(tokio::fs::File::create(&dst).await?);
                    writer.write_all(&res).await?;
                    writer.flush().await?;
                    Result::Ok(())
                } else {
                    break;
                }
            };
            match res {
                Ok(_) => return Ok(()),
                Err(x) => {
                    println!("{} failed,retry [{}/3],message {:?}", src, try_time + 1, x);
                }
            }
        }
        Result::Err(format!("{} failed 3 time", src).into())
    }

    pub fn get_icon(
        &self,
        source: Vec<(String, String)>,
        base: &std::path::Path,
        status: &mut i32,
    ) {
        self.rt.block_on(async {
            *status = crate::typing::Status::UpdateSkillIcon as i32;
            let type_stream = source.into_iter().map(|(dst, src)| async move {
                let dst = base.join(dst);
                let src = format!("http://seerh5.61.com/resource/assets/PetType/{}", src);
                self.get_single(src, dst).await?;
                Result::Ok(())
            });
            let _ = join_all(type_stream).await;
        });
    }

    pub fn get_blob(&self, source: Vec<i32>, base: &std::path::Path, status: &mut i32) {
        self.rt.block_on(async {
            let mut done = 0;

            let mut head_stream = futures::stream::iter(source.into_iter().map(|x| async move {
                let dst = base.join(format!("{}.png", x));
                let src = format!("http://seerh5.61.com/resource/assets/pet/head/{}.png", x);
                self.get_single(src, dst).await?;
                Result::Ok(())
            }))
            .buffered(200);

            while let Some(x) = head_stream.next().await {
                if x.is_ok() {
                    done += 1;
                    *status = done;
                }
            }
        });
    }

    pub fn get_res(&self) -> Vec<String> {
        self.rt.block_on(async {
            let version = self
                .opener
                .get("http://seerh5.61.com/version/version.json")
                .send()
                .await
                .unwrap()
                .text()
                .await
                .unwrap();
            let version = json::parse(&version).unwrap();
            let keyword = vec![
                "monsters.json",
                "moves.json",
                "side_effect.json",
                "effectInfo.json",
                "skillTypes.json",
                "hide_moves.json",
                "effectIcon.json",
                "mintmark.json",
            ];

            let pack = keyword.into_iter().map(|x| {
                let str = version["files"]["resource"]["config"]["xml"][x]
                    .as_str()
                    .unwrap();
                let url = format!("http://seerh5.61.com/resource/config/xml/{}", str);
                self.opener.get(url).send()
            });

            let pack = join_all(pack).await;
            let pack = pack.into_iter().map(|x| x.unwrap().text());

            let pack = join_all(pack).await;

            let mut ret: Vec<String> = pack.into_iter().map(|x| x.unwrap()).collect();
            let res = version["files"]["resource"]["assets"]["PetType"].to_string();
            ret.push(res);
            ret
        })
    }
}

pub struct PetBookManager {
    pub magic: i32,
    net: NetworkManager,
    pub status: i32,
}

impl PetBookManager {
    pub fn get_db() -> Connection {
        Connection::open_with_flags(
            "core.db",
            OpenFlags::SQLITE_OPEN_READ_WRITE
                | OpenFlags::SQLITE_OPEN_CREATE
                | OpenFlags::SQLITE_OPEN_NO_MUTEX,
        )
        .unwrap()
    }
    pub fn new() -> Self {
        let db = Self::get_db();
        let sqls = vec![
            "create table if not exists pet
            (
                id integer primary key,
                name text not null,
                hp integer not null,
                atk integer not null,
                def integer not null,
                spatk integer not null,
                spdef integer not null,
                spd integer not null,
                type integer not null,
                gender integer not null,
                evolving_lv integer
            )",
            "create table if not exists pet_skill
            (
                pet integer not null,
                skill integer not null,
                lv integer not null,
                extra integer not null
            )",
            "create table if not exists skill
            (
                id integer not null,
                name text not null,
                category integer not null,
                type integer not null,
                power integer not null,
                pp integer not null,
                accuracy integer not null,
                critical_rate integer,
                effect text,
                effect_arg text,
                info text
            )",
            "create table if not exists effect
            (
                id integer primary key,
                description text not null,
                args_count integer
            )",
            "create table if not exists typing
            (
                id integer primary key,
                name text not null,
                is_double integer not null
            )",
            "create table if not exists buff
            (
                id integer primary key,
                effect_id integer,
                tips text,
                come text,
                url text
            )",
            "create table if not exists pet_buff
            (
                id integer,
                pet_id integer not null
            )",
            "create table if not exists mintmark
            (
                id integer primary key,
                name text not null,
                atk integer not null,
                def integer not null,
                spatk integer not null,
                spdef integer not null,
                spd integer not null,
                hp integer not null
            )",
        ];

        for sql in sqls {
            db.execute(sql, []).unwrap();
        }

        let net = NetworkManager::new();
        Self {
            magic: 114514,
            net,
            status: Status::Idle as i32,
        }
    }
    fn add_pet_skill(transaction: &Transaction, pet: &str, skill: &JsonValue, extras: bool) {
        let len = skill.len();
        for i in 0..len {
            let now = &skill[i];
            transaction
                .execute(
                    "INSERT INTO pet_skill (pet,skill,lv,extra) VALUES (?,?,?,?)",
                    [
                        pet.into(),
                        now["ID"].to_string(),
                        now["LearningLv"].to_string(),
                        (if extras { "1" } else { "0" }).into(),
                    ],
                )
                .unwrap();
        }
    }

    pub fn update(&mut self) {
        let mut db = Self::get_db();
        self.status = Status::UpdateDatabase as i32;
        let mut available_id: Vec<i32> = Vec::new();

        for i in [
            "pet",
            "pet_skill",
            "skill",
            "effect",
            "typing",
            "buff",
            "pet_buff",
            "mintmark",
        ] {
            db.execute(&format!("delete from {}", i), []).unwrap();
        }

        let base_path = std::path::Path::new("./blob");

        let transaction = db.transaction().unwrap();
        let res = self.net.get_res();
        let monsters = res.get(0).unwrap();
        let skill = res.get(1).unwrap();
        let side_effect = res.get(2).unwrap();
        let effect2 = res.get(3).unwrap();
        let types = res.get(4).unwrap();
        let hide_skill = res.get(5).unwrap();
        let buff = res.get(6).unwrap();
        let mintmark = res.get(7).unwrap();

        //last one
        let type_mapping = res.last().unwrap();

        //mintmark
        {
            let mintmark = json::parse(mintmark).unwrap();
            let mintmark = &mintmark["MintMarks"]["MintMark"];
            let len = mintmark.len();
            for i in 0..len {
                let now = &mintmark[i];
                let types = now["Type"].as_i32().unwrap();
                if types == 1 {
                    continue;
                }
                let id = now["ID"].to_string();
                let name = now["Des"].to_string();
                let arg = if types == 0 {
                    now["Arg"].to_string()
                } else {
                    now["MaxAttriValue"].to_string()
                };
                let extra = &now["ExtraAttriValue"];
                let extra = if extra.is_null() {
                    "0 0 0 0 0 0".to_string()
                } else {
                    extra.to_string()
                };
                let t1 = arg
                    .split(' ')
                    .map(|x| x.parse::<i32>().unwrap())
                    .collect::<Vec<i32>>();
                let t2 = extra
                    .split(' ')
                    .map(|x| x.parse::<i32>().unwrap())
                    .collect::<Vec<i32>>();
                let trunk: Vec<String> = (0..6)
                    .map(|x| {
                        let val = t1.get(x).unwrap() + t2.get(x).unwrap();
                        val.to_string()
                    })
                    .collect();
                transaction
                    .execute(
                        "INSERT INTO mintmark (id,name,atk,def,spatk,spdef,spd,hp) VALUES (?,?,?,?,?,?,?,?)",
                        [&id, &name, trunk.get(0).unwrap(), trunk.get(1).unwrap()
                            , trunk.get(2).unwrap(), trunk.get(3).unwrap(),
                            trunk.get(4).unwrap(), trunk.get(5).unwrap()
                        ],
                    )
                    .unwrap();
            }
        }
        //skill
        {
            let hide_skill = json::parse(hide_skill).unwrap();
            let hide_skill = &hide_skill["root"]["item"];
            let len = hide_skill.len();
            for i in 0..len {
                let now = &hide_skill[i];
                let move_id = now["moveId"].as_i32().unwrap();
                let pet_id = now["petId"].as_i32().unwrap();
                transaction
                    .execute(
                        "INSERT INTO pet_skill (pet,skill,lv,extra ) VALUES (?,?,?,?)",
                        [
                            pet_id.to_string(),
                            move_id.to_string(),
                            "0".to_string(),
                            "1".to_string(),
                        ],
                    )
                    .unwrap();
            }

            let monsters = json::parse(monsters).unwrap();
            let len = monsters["Monsters"]["Monster"].len();
            for i in 0..len {
                let now = &monsters["Monsters"]["Monster"][i];
                let learn = &now["LearnableMoves"]["Move"];
                let ext = &now["ExtraMoves"]["Move"];

                let params = [
                    "ID",
                    "DefName",
                    "HP",
                    "Atk",
                    "Def",
                    "SpAtk",
                    "SpDef",
                    "Spd",
                    "Type",
                    "Gender",
                    "EvolvingLv",
                ]
                .map(|x| {
                    let cur = &now[x];
                    if cur.is_null() {
                        "null".into()
                    } else {
                        cur.to_string()
                    }
                });
                let id = params[0].clone();
                available_id.push(id.parse().unwrap());
                transaction
                    .execute(
                        "INSERT INTO pet (id,name,hp,atk,def,spatk,spdef,spd,type,gender,evolving_lv) \
                VALUES (?,?,?,?,?,?,?,?,?,?,?)",
                        params,
                    )
                    .unwrap();

                Self::add_pet_skill(&transaction, &id, learn, false);
                Self::add_pet_skill(&transaction, &id, ext, true);
            }
            let skill = json::parse(skill).unwrap();
            let skills = &skill["MovesTbl"]["Moves"]["Move"];
            let len = skills.len();
            for i in 0..len {
                let now = &skills[i];
                let params = [
                    "ID",
                    "Name",
                    "Category",
                    "Type",
                    "Power",
                    "MaxPP",
                    "Accuracy",
                    "CritRate",
                    "SideEffect",
                    "SideEffectArg",
                    "info",
                ]
                .map(|x| {
                    let cur = &now[x];
                    if cur.is_null() {
                        "null".into()
                    } else {
                        cur.to_string()
                    }
                });
                transaction
                    .execute(
                        "INSERT INTO skill (id,name,category,type,power,pp,accuracy,critical_rate,effect,effect_arg,info) \
                VALUES (?,?,?,?,?,?,?,?,?,?,?)",
                        params,
                    ).unwrap();
            }

            //effect
            let mut mp = HashMap::new();

            let effect = &skill["MovesTbl"]["SideEffects"]["SideEffect"];
            let len = effect.len();
            for i in 0..len {
                let now = &effect[i];
                let id = now["ID"].to_string();
                let id = id
                    .chars()
                    .into_iter()
                    .filter(|x| x.is_digit(10))
                    .collect::<String>()
                    .parse::<i32>()
                    .unwrap()
                    - 1000000;
                mp.entry(id).or_insert_with(|| now["des"].to_string());
            }

            let effect = json::parse(effect2).unwrap();
            let effect = &effect["root"]["Effect"];
            let len = effect.len();
            for i in 0..len {
                let now = &effect[i];
                let id = now["id"].as_i32().unwrap();
                mp.entry(id).or_insert_with(|| now["info"].to_string());
            }

            let side_effect = json::parse(side_effect).unwrap();
            let side_effect = &side_effect["SideEffects"]["SideEffect"];
            let len = side_effect.len();
            for i in 0..len {
                let now = &side_effect[i];
                let id: i32 = now["ID"].as_i32().unwrap();
                let cnt = now["SideEffectArgcount"].as_i32().unwrap();
                let prev = mp.get(&id);
                if let Some(x) = prev {
                    let _ = transaction.execute(
                        "INSERT INTO effect (id,description,args_count) VALUES (?,?,?)",
                        [id.to_string(), x.to_string(), cnt.to_string()],
                    );
                }
            }
        }

        //buff
        {
            let buff = json::parse(buff).unwrap();
            let buff = &buff["root"]["effect"];
            let len = buff.len();
            for i in 0..len {
                let now = &buff[i];
                let params = ["Id", "iconId", "tips", "come", "url"].map(|x| now[x].to_string());
                let id = params[0].to_string();
                let pet_id = now["petId"].to_string();

                transaction
                    .execute(
                        "INSERT INTO buff (id,effect_id,tips,come,url) VALUES (?,?,?,?,?)",
                        params,
                    )
                    .unwrap();

                let x = pet_id.split('/').collect::<Vec<_>>();
                for j in x {
                    transaction
                        .execute("INSERT INTO pet_buff (id,pet_id) VALUES (?,?)", [&id, j])
                        .unwrap();
                }
            }
        }
        //update blob
        {
            if !base_path.exists() {
                std::fs::create_dir(base_path).unwrap();
            }
            self.net.get_blob(available_id, base_path, &mut self.status);
        }
        //update icon
        {
            let mut v = Vec::new();
            let types = json::parse(types).unwrap();
            let types = &types["root"]["item"];
            let len = types.len();
            for i in 0..len {
                let now = &types[i];
                let id = now["id"].as_i32().unwrap();
                let is_double = &now["is_dou"];
                let is_double = !is_double.is_null() && is_double.as_i32().unwrap() == 1;
                let is_double = if is_double { 1 } else { 0 };
                let name = now["cn"].to_string();

                transaction
                    .execute(
                        "INSERT INTO typing (id,name,is_double) VALUES (?,?,?)",
                        [id.to_string(), name, is_double.to_string()],
                    )
                    .unwrap();

                v.push(id);
            }
            let type_mapping = json::parse(type_mapping).unwrap();
            let mut v = if type_mapping.is_null() {
                v.iter()
                    .map(|x| (format!("type_{}.png", x), format!("{}.png", x)))
                    .collect::<Vec<(String, String)>>()
            } else {
                v.iter()
                    .map(|x| {
                        let matching = format!("{}.png", x);
                        if type_mapping[&matching].is_null() {
                            (format!("type_{}.png", x), matching)
                        } else {
                            (
                                format!("type_{}.png", x),
                                type_mapping[&matching].to_string(),
                            )
                        }
                    })
                    .collect::<Vec<(String, String)>>()
            };
            v.push(("prop.png".to_string(), "prop.png".to_string()));
            self.net.get_icon(v, base_path, &mut self.status);
        }
        transaction.commit().unwrap();
    }
    pub fn query_skill_type(&self) -> Array {
        let db = Self::get_db();
        let mut stmt = db.prepare("SELECT * from typing").unwrap();
        let mut rows = stmt.query([]).unwrap();
        let mut ret = Vec::new();
        while let Some(row) = rows.next().unwrap() {
            let id: i32 = row.get::<usize, i32>(0).unwrap();
            let name: String = row.get::<usize, String>(1).unwrap().to_string();
            let is_double = row.get::<usize, u8>(2).unwrap();
            ret.push(SkillType {
                id,
                name: CString::new(name).unwrap().into_raw(),
                is_double,
            });
        }
        Array::new(ret)
    }
    pub fn query_pet_list(&self, order: OrderBy) -> Array {
        let s = format!("SELECT id,name from pet where id<100000 {}", order.to_str());
        let arg = [];
        self.query_int_string_pair_list(&s, arg)
    }

    pub fn query_pet_list_with_condition(&self, str: &str, order: OrderBy) -> Array {
        let sb = format!("%{}%", str);
        let s = format!(
            "SELECT id,name from pet where (name like ? or cast(id as text) like ?) and id<100000 {}",
            order.to_str()
        );
        let arg = [&sb, &sb];
        self.query_int_string_pair_list(&s, arg)
    }

    pub fn query_pet_detail(&self, idx: i32) -> PetFullDetail {
        let db = Self::get_db();
        let mut stmt = db.prepare("SELECT * from pet where id = ?").unwrap();
        let mut row = stmt.query([idx]).unwrap();
        let row = row.next().unwrap().unwrap();
        let id: u32 = row.get::<usize, u32>(0).unwrap();
        let name: String = row.get::<usize, String>(1).unwrap();
        let hp: u32 = row.get::<usize, u32>(2).unwrap_or(0);
        let atk: u32 = row.get::<usize, u32>(3).unwrap_or(0);
        let def: u32 = row.get::<usize, u32>(4).unwrap_or(0);
        let spatk: u32 = row.get::<usize, u32>(5).unwrap_or(0);
        let spdef: u32 = row.get::<usize, u32>(6).unwrap_or(0);
        let spd: u32 = row.get::<usize, u32>(7).unwrap_or(0);
        let types: u32 = row.get::<usize, u32>(8).unwrap_or(0);
        let gender: u32 = row.get::<usize, u32>(9).unwrap_or(0);
        let evolving_lv: u32 = row.get::<usize, u32>(10).unwrap_or(0);
        let base = BaseDetail::new(id, name);
        PetFullDetail {
            base,
            hp,
            atk,
            def,
            spatk,
            spdef,
            spd,

            types,
            gender,
            evolving_lv,
        }
    }
    pub fn parse_skill(&self, row: &Row) -> Skill {
        let db = Self::get_db();
        let id: i32 = row.get::<usize, i32>(0).unwrap();
        let name = row.get::<usize, String>(1).unwrap();
        let category: i32 = row.get::<usize, i32>(2).unwrap_or(0);
        let types: i32 = row.get::<usize, i32>(3).unwrap_or(0);
        let power: i32 = row.get::<usize, i32>(4).unwrap_or(0);
        let pp: i32 = row.get::<usize, i32>(5).unwrap_or(0);
        let accuracy: i32 = row.get::<usize, i32>(6).unwrap_or(0);
        let critical_rate: i32 = row.get::<usize, i32>(7).unwrap_or(0);

        let effect = row.get::<usize, String>(8).unwrap();
        let mut effect_arg = row.get::<usize, String>(9).unwrap();
        if effect_arg == *"null" {
            effect_arg = "0".to_string();
        }
        let info = row.get::<usize, String>(10).unwrap();

        let lv: i32 = row.get::<usize, i32>(13).unwrap_or(0);
        let extra: i32 = row.get::<usize, i32>(14).unwrap_or(0);
        let mut effect_pack = Vec::new();
        if effect != *"null" {
            let effects = effect.split_whitespace();
            let effect_args = effect_arg.split_whitespace().collect::<Vec<_>>();

            let mut done = 0;
            for i in effects {
                let id: i32 = i.parse().unwrap();
                let mut stmt = db.prepare("SELECT * FROM 'effect' where id = ?").unwrap();
                let mut row = stmt.query([id]).unwrap();
                if let Ok(Some(row)) = row.next() {
                    let des = row.get::<usize, String>(1).unwrap().to_string();
                    let cnt = row.get::<usize, i32>(2).unwrap();
                    let args = (done..(done + cnt))
                        .map(|x| effect_args.get(x as usize).unwrap_or(&"0").to_string())
                        .collect::<Vec<_>>()
                        .join(" ");
                    let packed = Effect {
                        id,
                        cnt,
                        args: CString::new(args).unwrap().into_raw(),
                        description: CString::new(des).unwrap().into_raw(),
                    };
                    done += cnt;
                    effect_pack.push(packed);
                }
            }
        }

        unsafe {
            Skill {
                id,
                name: CString::new(name).unwrap().into_raw(),
                category,
                types,
                power,
                pp,
                accuracy,
                critical_rate,
                effect: std::mem::transmute(Box::leak(Box::new(Array::new(effect_pack)))),
                info: CString::new(info).unwrap().into_raw(),
                lv,
                extra,
            }
        }
    }
    pub fn query_pet_skill(&self, idx: i32) -> Array {
        let db = Self::get_db();
        let mut stmt = db.prepare(
            "SELECT * FROM skill as A cross join (SELECT * from pet_skill where pet = ?) as B where A.id=B.skill",
        ).unwrap();
        let mut rows = stmt.query([idx]).unwrap();
        let mut ret = Vec::new();
        while let Some(row) = rows.next().unwrap() {
            ret.push(self.parse_skill(row));
        }
        ret.sort_by(|a, b| {
            if a.extra != 0 && b.extra != 0 {
                Equal
            } else if a.extra != 0 {
                Less
            } else if b.extra != 0 {
                Greater
            } else {
                b.lv.cmp(&a.lv)
            }
        });
        Array::new(ret)
    }
    pub fn parse_buff(row: &Row) -> Buff {
        let id: i32 = row.get::<usize, i32>(0).unwrap();
        let effect_id = row.get::<usize, i32>(1).unwrap();
        let tips = row.get::<usize, String>(2).unwrap();
        let come = row.get::<usize, String>(3).unwrap();
        let url = row.get::<usize, String>(4).unwrap();
        Buff {
            id,
            effect_id,
            tips: CString::new(tips).unwrap().into_raw(),
            come: CString::new(come).unwrap().into_raw(),
            url: CString::new(url).unwrap().into_raw(),
        }
    }
    pub fn query_pet_buff(&self, idx: i32) -> Array {
        let db = Self::get_db();
        let mut stmt = db
            .prepare("select * from buff where id in (SELECT id FROM 'pet_buff' where pet_id=?)")
            .unwrap();
        let mut rows = stmt.query([idx]).unwrap();
        let mut ret = Vec::new();
        while let Some(row) = rows.next().unwrap() {
            ret.push(Self::parse_buff(row));
        }
        Array::new(ret)
    }

    pub fn query_buff(&self, idx: i32) -> Option<Buff> {
        let db = Self::get_db();
        let mut stmt = db.prepare("select * from buff where id = ?").unwrap();
        let mut rows = stmt.query([idx]).unwrap();
        rows.next().unwrap().map(Self::parse_buff)
    }

    pub fn query_skill_effect(&self, idx: i32) -> Option<BaseDetail> {
        let db = Self::get_db();
        let mut stmt = db
            .prepare("SELECT args_count,description from effect where id = ?")
            .unwrap();
        let mut rows = stmt.query([idx]).unwrap();
        if let Some(row) = rows.next().unwrap() {
            let s: u32 = row.get::<usize, u32>(0).unwrap();
            let b: String = row.get::<usize, String>(1).unwrap();
            Some(BaseDetail::new(s, b))
        } else {
            None
        }
    }

    pub fn query_skill_list_with_condition(&self, str: &str) -> Array {
        let sb = format!("%{}%", str);
        let s =
            "SELECT id,name from skill where name like ? or cast(id as text) like ? order by id";
        let arg = [&sb, &sb];
        self.query_int_string_pair_list(s, arg)
    }
    pub fn query_skill_detail(&self, idx: i32) -> Skill {
        let db = Self::get_db();
        let mut stmt = db.prepare("SELECT * from skill where id = ?").unwrap();
        let mut rows = stmt.query([idx]).unwrap();
        let row = rows.next().unwrap().unwrap();
        self.parse_skill(row)
    }

    pub fn query_skill_related(&self, idx: i32) -> Array {
        let db = Self::get_db();
        let mut stmt = db
            .prepare("SELECT pet from pet_skill where skill = ?")
            .unwrap();
        let mut rows = stmt.query([idx]).unwrap();
        let mut vcc = Vec::new();
        while let Some(row) = rows.next().unwrap() {
            let pet_id: u32 = row.get::<usize, u32>(0).unwrap();
            let mut stmt2 = db.prepare("SELECT name from pet where id = ?").unwrap();
            let name: String = stmt2
                .query_row([pet_id], |x| x.get::<usize, String>(0))
                .unwrap();
            vcc.push(BaseDetail::new(pet_id, name));
        }
        Array::new(vcc)
    }

    pub fn query_skill_list(&self) -> Array {
        let s = "SELECT id,name from skill order by id";
        let arg = [];
        self.query_int_string_pair_list(s, arg)
    }

    pub fn query_skill_effect_related(&self, idx: i32) -> Array {
        let s = "SELECT id,name from skill where (effect like ? or effect like ? or effect like ? or effect like ?)";
        let arg = [
            format!("{}", idx),
            format!("% {}", idx),
            format!("{} %", idx),
            format!("% {} %", idx),
        ];
        self.query_int_string_pair_list(s, arg)
    }

    pub fn query_buff_related(&self, idx: i32) -> Array {
        let s = "select B.id,B.name from (select pet_id from pet_buff where id = ?) as A cross join (pet) as B where A.pet_id = B.id";
        let arg = [idx];
        self.query_int_string_pair_list(s, arg)
    }

    pub fn query_int_string_pair_list<T: Params>(&self, str: &str, arg: T) -> Array {
        let db = Self::get_db();
        let mut ret = Vec::new();
        if let Ok(mut stmt) = db.prepare(str) {
            let mut rows = stmt.query(arg).unwrap();
            while let Some(row) = rows.next().unwrap() {
                let s: u32 = row.get::<usize, u32>(0).unwrap();
                let b: String = row.get::<usize, String>(1).unwrap().to_string();
                ret.push(BaseDetail::new(s, b));
            }
        }
        Array::new(ret)
    }
    fn query_mintmark_impl<T: Params>(&self, str: &str, arg: T) -> Array {
        let db = Self::get_db();
        let mut ret = Vec::new();
        if let Ok(mut stmt) = db.prepare(str) {
            let mut rows = stmt.query(arg).unwrap();
            while let Some(row) = rows.next().unwrap() {
                let id: i32 = row.get::<usize, i32>(0).unwrap();
                let name: String = row.get::<usize, String>(1).unwrap().to_string();
                let atk: i32 = row.get::<usize, i32>(2).unwrap();
                let def: i32 = row.get::<usize, i32>(3).unwrap();
                let spatk: i32 = row.get::<usize, i32>(4).unwrap();
                let spdef: i32 = row.get::<usize, i32>(5).unwrap();
                let spd: i32 = row.get::<usize, i32>(6).unwrap();
                let hp: i32 = row.get::<usize, i32>(7).unwrap();
                ret.push(MintMark {
                    id,
                    name: CString::new(name).unwrap().into_raw(),
                    atk,
                    def,
                    spatk,
                    spdef,
                    spd,
                    hp,
                });
            }
        }
        Array::new(ret)
    }

    pub fn query_mintmark_list(&self, order: OrderBy) -> Array {
        let s = format!("SELECT * from mintmark {}", order.to_str());
        let arg = [];
        self.query_mintmark_impl(&s, arg)
    }

    pub fn query_mintmark_list_with_condition(&self, str: &str, order: OrderBy) -> Array {
        let sb = format!("%{}%", str);
        let s = format!(
            "SELECT * from mintmark where name like ? {}",
            order.to_str()
        );
        let arg = [sb];
        self.query_mintmark_impl(&s, arg)
    }
}

impl Drop for PetBookManager {
    fn drop(&mut self) {
        println!("destroyed");
    }
}
