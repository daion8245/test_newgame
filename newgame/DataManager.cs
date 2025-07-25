﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection.Metadata;

namespace newgame
{
    public class DataManager
    {
        static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new DataManager();
                }

                return instance;
            }
        }

        #region 저장 & 불러오기
        public void Save(Status playerData)
        {
            string path, data;
            #region Player
            path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            data = JsonConvert.SerializeObject(playerData);
            File.WriteAllText(path, data);
            #endregion

            #region Inventory
            path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Inventory.json");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            data = JsonConvert.SerializeObject(Inventory.Instance, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            });

            File.WriteAllText(path, data);
            #endregion
        }

        public Status Load()
        {
            #region Player
            Status playerData = new Status();

            string path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");

            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                playerData = JsonConvert.DeserializeObject<Status>(data);
            }
            #endregion

            #region Inventory
            Inventory.Instance.Load();
            #endregion

            return playerData;
        }

        public bool IsPlayerData()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");

            return File.Exists(path);
        }

        public void DeleteData()
        {
            List<string> datas = new List<string>();
            string playerData = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Player.json");
            datas.Add(playerData);
            string inventoryData = Path.Combine(Directory.GetCurrentDirectory(), "GameData_Inventory.json");
            datas.Add(inventoryData);

            foreach(string path in datas)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        #endregion

        #region 착용 장비
        /// <summary>
        /// 착용 장비 데이터 불러오기
        /// </summary>
        public void LoadAllEquipData()
        {
            // exe 파일 실행 경로
            string exePath = AppDomain.CurrentDomain.BaseDirectory;

            for(int i = 1; i < (int)EquipType.MAX; i++)
            {
                // 텍스트 파일 이름
                string fileName = $"Equip_{(EquipType)i}.txt";
                // 텍스트 파일 경로
                string filePath = Path.Combine(exePath, fileName);

                // 텍스트 파일 존재 유무 판단
                if(!File.Exists(filePath))
                {
                    // 파일이 없는 경우
                    Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                    Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                    return;
                }

                SetEquipData(filePath, (EquipType)i);
            }
        }

        void SetEquipData(string filePath, EquipType _type)
        {
            try
            {
                // 텍스트 파일에서 모든 라인 읽어오기
                string[] lines = File.ReadAllLines(filePath);
                string name = string.Empty;         // 아이템 이름
                int[] data = new int[3];            // id, stat, price
                foreach(string line in lines)
                {
                    if(line == "#")
                    {
                        // 현재까지 얻어진 정보로 Equipment 클래스 생성하고
                        Equipment equip = new Equipment(_type, data[0], name, data[1], data[2]);
                        // GameManager 에서 equips 리스트에 등록하기
                        GameManager.Instance.SetEquipList(equip);

                        continue;
                    }

                    // 문자열 자르기 ( 해당 형식은 ":" 를 기준으로 문자열을 구분하고 있음 )
                    string[] curLine = line.Split(':');
                    if (curLine[0].Trim() == "ID")                 // ID 일 때
                    {
                        data[0] = int.Parse(curLine[1].Trim());
                    }
                    else if(curLine[0].Trim() == "NAME")           // NAME 일 때
                    {
                        name = curLine[1].Trim();
                    }
                    else if(curLine[0].Trim() == "STAT")           // STAT 일 때
                    {
                        data[1] = int.Parse(curLine[1].Trim());
                    }
                    else if(curLine[0].Trim() == "PRICE")          // PRICE 일 때
                    {
                        data[2] = int.Parse(curLine[1].Trim());
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[오류] : 파일 읽기 실패 ({ex.Message})");
            }
        }
        #endregion

        #region 몬스터
        public void LoadEnemyData()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;

            // 텍스트 파일 이름
            string fileName = $"Monster.txt";
            // 텍스트 파일 경로
            string filePath = Path.Combine(exePath, fileName);

            // 파일 체크
            if(File.Exists(filePath) == false)
            {
                // 파일이 없는 경우
                Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                return;
            }

            SetEnemyData(filePath);
        }

        void SetEnemyData(string filePath)
        {
            try
            {
                // 텍스트 파일에서 모든 라인 읽어오기
                string[] lines = File.ReadAllLines(filePath);

                Status monStat = new Status();

                foreach (string line in lines)
                {
                    if (line == "#")
                    {
                        // GameManager 에서 monster 리스트에 등록하기
                        GameManager.Instance.SetMonsterInfo(monStat);
                        monStat = new Status();
                        continue;
                    }

                    // 문자열 자르기 ( 해당 형식은 ":" 를 기준으로 문자열을 구분하고 있음 )
                    string[] curLine = line.Split(':');
                    //if (curLine[0].Trim() == "TYPE")                 // Type 일 때
                    //{
                    //    monStat.charType = (CharType)Enum.Parse(typeof(CharType), curLine[1].Trim());
                    //}
                    if (curLine[0].Trim() == "NAME")           // NAME 일 때
                    {
                        monStat.Name = curLine[1].Trim();
                    }
                    else if (curLine[0].Trim() == "LEVEL")           // STAT 일 때
                    {
                        monStat.level = int.Parse(curLine[1].Trim());
                    }
                    else if (curLine[0].Trim() == "hp")          // PRICE 일 때
                    {
                        monStat.hp = int.Parse(curLine[1].Trim());
                        monStat.maxHp = monStat.hp;
                    }
                    else if (curLine[0].Trim() == "ATK")
                    {
                        monStat.ATK = int.Parse(curLine[1].Trim());
                    }
                    else if (curLine[0].Trim() == "DEF")
                    {
                        monStat.DEF = int.Parse(curLine[1].Trim());
                    }
                    else if (curLine[0].Trim() == "EXP")
                    {
                        monStat.exp = int.Parse(curLine[1].Trim());
                    }
                    else if (curLine[0].Trim() == "gold")
                    {
                        monStat.gold = int.Parse(curLine[1].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류] : 파일 읽기 실패 ({ex.Message})");
            }
        }
        #endregion

        #region 던전 맵
        public void LoadDungeonMap()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = $"Dungeon_Map.txt";
            string filePath = Path.Combine(exePath, fileName);

            if (File.Exists(filePath) == false)
            {
                // 파일이 없는 경우
                Console.WriteLine($"해당 경로 [{filePath}]가 존재하지 않습니다. ");
                Console.WriteLine($"[{fileName}] 파일을 확인해주세요.");
                return;
            }

            SetDungeonMapData(filePath);
        }

        void SetDungeonMapData(string filePath)
        {
            var mapRows = new List<List<int>>();
            foreach (string raw in File.ReadLines(filePath))
            {
                string line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line == "#")
                {
                    if (mapRows.Count > 0)
                        GameManager.Instance.SetDungeonMapInfo(mapRows);
                    mapRows = new List<List<int>>();
                    continue;
                }

                // 콤마 단위로 잘라 int 리스트로 변환
                List<int> row = line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => int.Parse(s.Trim()))
                                    .ToList();
                mapRows.Add(row);
            }
            if (mapRows.Count > 0)
                GameManager.Instance.SetDungeonMapInfo(mapRows);
        }

        #endregion
    }
}