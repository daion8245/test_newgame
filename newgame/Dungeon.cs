﻿using static newgame.UiHelper;

namespace newgame
{
    internal class Dungeon
    {

        enum RoomType
        {
            Wall,
            Empty,
            Ladder,
            Monster,
            Treasure,
            Shop,
            Event,
            Boss,
            Exit
        }

        const int Padding = 1;               // 각 방 주변 공백 크기
        const int CellSize = Padding * 2 + 1; // 실제 화면에 사용될 셀 크기

        public void Start()
        {
            Console.Clear();
            LoadMapData();
            SetDungeon();
        }

        #region 던전

        // 맵 데이터 (2차원 배열)
        List<List<int>> map = new List<List<int>>();

        void LoadMapData()
        {
            map = GameManager.Instance.GetDungeonMap(1);
        }

        // 플레이어 위치
        static int playerX = 1, playerY = 1;

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

                // 현재 방 정보 출력
                Console.WriteLine();
                Console.WriteLine("현재 방: " + GetRoomName((RoomType)map[playerY][playerX]));
                RoomEvent((RoomType)map[playerY][playerX]);


                // 키 입력 받기
                ConsoleKeyInfo key = Console.ReadKey(true);

                // 이동 처리
                int newX = playerX, newY = playerY;

                if (key.Key == ConsoleKey.UpArrow) newY--;      // 위로
                else if (key.Key == ConsoleKey.DownArrow) newY++; // 아래로
                else if (key.Key == ConsoleKey.LeftArrow) newX--; // 왼쪽으로
                else if (key.Key == ConsoleKey.RightArrow) newX++; // 오른쪽으로

                // 이동 가능한지 확인 (맵 안에 있고 벽이 아닌 경우)
                if (newX >= 0 && newX < width && newY >= 0 && newY < height && map[newY][newX] != 0)
                {
                    playerX = newX;
                    playerY = newY;
                }
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
            RoomType room = (RoomType)map[playerY][playerX]; // 수정
            switch (room)
            {
                case RoomType.Monster:
                    CreateMonster();
                    break;
                case RoomType.Treasure:
                    Console.WriteLine("보물을 찾았습니다!");
                    // 보물 획득 로직 추가
                    break;
                case RoomType.Shop:
                    Console.WriteLine("상점에 들어왔습니다.");
                    // 상점 로직 추가
                    break;
                case RoomType.Event:
                    Console.WriteLine("특별 이벤트가 발생했습니다!");
                    // 이벤트 로직 추가
                    break;
                case RoomType.Boss:
                    Console.WriteLine("보스와의 전투가 시작됩니다!");
                    CreateMonster(); // 보스 몬스터 생성
                    break;
                case RoomType.Exit:
                    Console.WriteLine("던전을 클리어했습니다!");
                    // 게임 종료 또는 다음 단계로 이동
                    break;
                default:
                    Console.WriteLine("빈 방입니다.");
                    break;
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

        void DrawRoomWithPadding(RoomType room, int x, int y)
        {
            int left = x * CellSize;
            int top = y * CellSize;
            char symbol = GetRoomSymbol(room);

            if (room == RoomType.Wall)
            {
                for (int i = 0; i < CellSize; i++)
                {
                    Console.SetCursorPosition(left, top + i);
                    Console.Write(new string(symbol, CellSize));
                }
            }
            else
            {
                for (int i = 0; i < CellSize; i++)
                {
                    Console.SetCursorPosition(left, top + i);
                    Console.Write(new string(' ', CellSize));
                }
                Console.SetCursorPosition(left + Padding, top + Padding);
                Console.Write(symbol);
            }
        }

        void DrawMap(int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    DrawRoomWithPadding((RoomType)map[y][x], x, y);
                }
            }
        }

        void DrawPlayer()
        {
            int left = playerX * CellSize + Padding;
            int top = playerY * CellSize + Padding;
            Console.SetCursorPosition(left, top);
            Console.Write('@');
        }
        #endregion

        #endregion

        #region 몬스터 소환/배틀
        void CreateMonster()
        {
            Monster monster = new Monster();
            GameManager.Instance.monster = monster;
            monster.Start();
            Battle();
        }

        void Battle()
        {
            Battle battle = new Battle();
            battle.Start();
        }
        #endregion
    }
}
