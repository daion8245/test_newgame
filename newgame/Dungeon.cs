using System.Drawing;
using static newgame.UiHelper;

namespace newgame
{
    internal class Dungeon
    {
        private static readonly Dungeon _instance = new Dungeon();
        public static Dungeon Instance { get { return _instance; } }

        private Dungeon() { }

        public int Floor { get; private set; } = 1;
        public void NextFloor() { Floor++; }

        enum RoomType
        {
            Wall, //0
            Empty, //1
            Ladder, //2
            Monster, //3
            Treasure, //4
            Shop, //5
            Event, //6
            Boss, //7
            Exit //8
        }

        public static int floor = 1; // 현재 층수

        public void Start()
        {
            Console.Clear();
            LoadMapData(floor);
            SetDungeon();
        }

        #region 던전

        // 맵 데이터 (2차원 배열)
        List<List<int>> map = new List<List<int>>();
        void LoadMapData(int number)
        {
            map = GameManager.Instance.GetDungeonMap(number);
            NormalizeToRectangle(); // 맵을 직사각형으로 정규화
        }

        // 플레이어 위치
        static Point player = new Point(1,1);

        void SetDungeon()
        {
            int height = map.Count;
            int width = map[0].Count;

            // 게임 시작
            while (true)
            {
                Console.Clear();

                DrawMap(width, height);
                DrawPlayer();

                // 키 입력 받기
                ConsoleKeyInfo key = Console.ReadKey(true);
                
                // 이동 처리
                int newX = player.X, newY = player.Y;

                if (key.Key == ConsoleKey.UpArrow) newY--;      // 위로
                else if (key.Key == ConsoleKey.DownArrow) newY++; // 아래로
                else if (key.Key == ConsoleKey.LeftArrow) newX--; // 왼쪽으로
                else if (key.Key == ConsoleKey.RightArrow) newX++; // 오른쪽으로

                // 이동 가능한지 확인 (맵 안에 있고 벽이 아닌 경우)
                if (newX >= 0 && newX < width && newY >= 0 && newY < height && map[newY][newX] != 0)
                {
                    player.X = newX;
                    player.Y = newY;
                }

                RoomEvent((RoomType)map[player.Y][player.X]);

                if (HandlePlayerDefeatIfNeeded())
                {
                    return;
                }

                Player? activePlayerForCleanup = GameManager.Instance.Player;
                if (activePlayerForCleanup != null && !activePlayerForCleanup.IsDead)
                {
                    RoomDelete();
                }

                GameManager.Instance.UpdateDungeonMap(floor, map);
            }

        }
        #region 방 이름 가져오기
        static string GetRoomName(RoomType room)
        {
            switch (room)
            {
                case RoomType.Wall: return "벽";
                case RoomType.Empty: return "빈 방";
                case RoomType.Ladder: return "사다리";
                case RoomType.Monster: return "몬스터";
                case RoomType.Treasure: return "보물";
                case RoomType.Shop: return "상점";
                case RoomType.Event: return "이벤트";
                case RoomType.Boss: return "보스";
                case RoomType.Exit: return "출구";
                default: return "알 수 없음";
            }
        }
        #endregion

        #region 방 이벤트 처리
        void RoomEvent(RoomType playerRoom)
        {
            RoomType room = (RoomType)map[player.Y][player.X];
            Console.SetCursorPosition(0, map.Count + 1);
            switch (room)
            {
                #region 몬스터
                case RoomType.Monster:
                    {
                        Console.Clear();
                        UiHelper.TxtOut(new string[]
                        {
                            "몬스터 방에 진입했습니다.",""
                        });

                        int select = UiHelper.SelectMenu(new string[]
                        {
                            "몬스터와 전투",
                            "도망 시도(35%)"
                        });

                        Player activePlayer = GameManager.Instance.RequirePlayer();

                        if(select == 0)
                        {
                            UiHelper.WaitForInput("몬스터와의 전투를 시작합니다. [ENTER를 눌러 계속]");
                            MonsterCreate();
                        }
                        else
                        {
                            int randomChance = new Random().Next(1, 101);
                            if (randomChance <= 35) // 35% 확률로 도망 성공
                            {
                                UiHelper.WaitForInput("몬스터 방에서 도망치는데 성공했습니다!  [ENTER를 눌러 계속]");
                            }
                            else
                            {
                                UiHelper.WaitForInput("도망에 실패했습니다! 체력의 30%를 잃고 몬스터와 전투를 시작합니다!  [ENTER를 눌러 계속]");
                                activePlayer.MyStatus.Hp -= (int)(activePlayer.MyStatus.MaxHp * 0.3);
                                MonsterCreate();
                            }
                        }
                        if (GameManager.Instance.monster?.IsDead ?? false)
                        {
                            RoomDelete(); // 승리 시 방 삭제
                        }
                        break;
                    }
                #endregion
                case (RoomType.Treasure):
                    {
                        // 보물 획득 로직 추가
                        break;
                    }
                case (RoomType.Shop):
                    {
                        // 상점 로직 추가
                        break;
                    }
                case (RoomType.Event):
                    {
                        // 이벤트 로직 추가
                        break;
                    }
                case (RoomType.Ladder):
                    {
                        Console.Clear();
                        UiHelper.WaitForInput($"사다리를 타고 다음 층({floor + 1}층) 으로 이동합니다. [ENTER를 눌러 계속]");
                        int currentFloor = floor;
                        GameManager.Instance.UpdateDungeonMap(currentFloor, map);
                        floor++; // 층수 증가
                        LoadMapData(floor); // 다음 층 맵 데이터 로드
                        player.X = 1; // 플레이어 위치 초기화
                        player.Y = 1; // 플레이어 위치 초기화
                        break;
                    }
                case (RoomType.Boss):
                    {
                        BossCreate(); // 보스 몬스터 생성
                        break;
                    }
                #region 마을로 돌아가기
                case (RoomType.Exit):
                    {
                        Console.Clear();
                        UiHelper.TxtOut(["던전에서 나가 마을로 돌아가시겠습니까?", ""]);
                        int sel = UiHelper.SelectMenu(["돌아가기","계속하기"]);
                        Console.WriteLine();
                        if (sel == 0)
                        {
                            UiHelper.WaitForInput("마을로 돌아갑니다. [ENTER를 눌러 계속]");
                            GameManager.Instance.UpdateDungeonMap(floor, map);
                            player.X = 1; // 플레이어 위치 초기화
                            player.Y = 1; // 플레이어 위치 초기화
                            GameManager.Instance.ReturnToLobby();
                        }
                        else
                        {
                            UiHelper.WaitForInput("던전 탐험을 계속합니다. [ENTER를 눌러 계속]");
                            player.Offset(1,1);
                        }
                        break;
                    }
                #endregion
                default:
                    {
                        break;
                    }
            }
        }
        #endregion

        #region 방 그리기
        char GetRoomSymbol(RoomType room)
        {
            return room switch
            {
                RoomType.Wall => '■',
                RoomType.Empty => ' ',
                RoomType.Ladder => '▲',
                RoomType.Monster => 'M',
                RoomType.Treasure => 'T',
                RoomType.Shop => 'S',
                RoomType.Event => 'E',
                RoomType.Boss => 'B',
                RoomType.Exit => 'X',
                _ => ' '
            };
        }

        void DrawMap(int width, int height)
        {
            for (int y = 0; y < map.Count; y++)
            {
                for (int x = 0; x < map[y].Count; x++)
                {
                    Console.Write(GetRoomSymbol((RoomType)map[y][x]));
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("현제 층: " + floor);
            Console.WriteLine("현재 방: " + GetRoomName((RoomType)map[player.Y][player.X]));
            Console.WriteLine();
            Console.WriteLine("[■ = 벽] [ = 빈 방] [▲ 사다리] [M = 몬스터] [T = 보물] [S = 상점] [E = 이벤트] [B = 보스] [X = 출구]");
            Console.WriteLine();
            // 인접 방 경계 체크
            Console.WriteLine($"\t↑{GetRoomName(GetRoomTypeSafe(player.Y - 1, player.X))}");
            Console.WriteLine($"←{GetRoomName(GetRoomTypeSafe(player.Y, player.X - 1))}" +
                              $"\t\t→{GetRoomName(GetRoomTypeSafe(player.Y, player.X + 1))}");
            Console.WriteLine($"\t↓{GetRoomName(GetRoomTypeSafe(player.Y + 1, player.X))}");
        }

        RoomType GetRoomTypeSafe(int y, int x)
        {
            if (y >= 0 && y < map.Count)
            {
                if (x >= 0 && x < map[y].Count)
                    return (RoomType)map[y][x];
                return RoomType.Empty;
            }

            return RoomType.Wall;
        }

        void NormalizeToRectangle()
        {
            int maxWidth = map.Max(r => r.Count);
            for (int i = 0; i < map.Count; i++)
            {
                while (map[i].Count < maxWidth)
                    map[i].Add((int)RoomType.Empty); // 비어 있는 칸으로 채움
            }
        }
        
        void DrawPlayer()
        {
            int left = player.X;
            int top = player.Y;
            Console.SetCursorPosition(left, top);
            Console.Write('@');
        }

        bool HandlePlayerDefeatIfNeeded()
        {
            Player? activePlayer = GameManager.Instance.Player;
            if (activePlayer == null || !activePlayer.IsDead)
            {
                return false;
            }

            GameManager.Instance.UpdateDungeonMap(floor, map);
            UiHelper.WaitForInput("던전에서 패배하여 마을로 돌아갑니다. [ENTER를 눌러 계속]");

            activePlayer.RespawnAtTavern();
            activePlayer.IsDead = false;

            player.X = 1; // 플레이어 위치 초기화
            player.Y = 1; // 플레이어 위치 초기화

            return true;
        }

        void RoomDelete()
        {
            if (player.Y >= 0 && player.Y < map.Count && player.X >= 0 && player.X < map[player.Y].Count &&
                (RoomType)map[player.Y][player.X] != RoomType.Empty &&
                (RoomType)map[player.Y][player.X] != RoomType.Exit)
            {
                map[player.Y][player.X] = (int)RoomType.Empty;
            }
        }
        #endregion

        #endregion

        #region 몬스터 소환/배틀
        void MonsterCreate()
        {
            Monster monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.Start(1);
            Battle battle = new Battle();
            battle.Start();
        }
        #endregion

        #region 보스 소환/배틀

        void BossCreate()
        {
            Boss boss = new Boss();
            GameManager.Instance.monster = boss;
            boss.StartBoss(floor);
            Battle battle = new Battle();
            battle.Start();
        }
        #endregion
    }
}