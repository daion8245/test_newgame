using System;
using System.Collections.Generic;
using SkiaSharp;

namespace newgame
{
    internal class Player : Character
    {
        private readonly Skills skillSystem;
        private readonly PlayerInitializer initializer;
        private CharacterClassType? currentClass;

        public Player() : this(GameManager.Instance.BattleLogService)
        {
        }

        public Player(BattleLogService battleLogService) : this(
            battleLogService,
            new Status(),
            Inventory.Instance,
            new Skills())
        {
        }

        internal Player(
            BattleLogService battleLogService,
            Status status,
            Inventory inventory,
            Skills skills) : base(battleLogService)
        {
            MyStatus = status ?? throw new ArgumentNullException(nameof(status));
            skillSystem = skills ?? throw new ArgumentNullException(nameof(skills));
            initializer = new PlayerInitializer(
                MyStatus,
                inventory ?? throw new ArgumentNullException(nameof(inventory)),
                skillSystem);
        }

        public PlayerInitializer Initializer => initializer;

        public CharacterClassType? CurrentClass => currentClass;

        public void AssignClass(CharacterClassType classType)
        {
            if (string.IsNullOrWhiteSpace(classType.name))
            {
                throw new ArgumentException("Class name cannot be empty.", nameof(classType));
            }

            if (currentClass != null)
            {
                if (string.Equals(currentClass.Value.name, classType.name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                throw new InvalidOperationException("Player already has a different class assigned.");
            }

            MyStatus.ApplyClass(classType);
            currentClass = classType;
            ApplyClassSkills(classType.name);
        }

        public bool TryAssignClass(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return false;
            }

            if (!GameManager.Instance.TryGetPlayerClass(className, out CharacterClassType classType))
            {
                return false;
            }

            AssignClass(classType);
            return true;
        }

        private void ApplyClassSkills(string className)
        {
            List<SkillType> classSkills = GameManager.Instance.GetClassSkills(className);
            if (classSkills.Count == 0)
            {
                return;
            }

            skillSystem.ClearAllCanUseSkills();
            foreach (SkillType skill in classSkills)
            {
                skillSystem.AddCanUseSkill(skill.name);
            }
        }

        private void RestoreClassState()
        {
            if (string.IsNullOrWhiteSpace(MyStatus.ClassName))
            {
                currentClass = null;
                return;
            }

            if (GameManager.Instance.TryGetPlayerClass(MyStatus.ClassName, out CharacterClassType classType))
            {
                currentClass = classType;
                ApplyClassSkills(classType.name);
            }
            else
            {
                currentClass = null;
            }
        }


        public void ApplyStatus(Status status)
        {
            MyStatus = status ?? throw new ArgumentNullException(nameof(status));
            initializer.AttachStatus(status);
            RestoreClassState();
        }

        #region 패배 후 복구
        public void RespawnAtTavern()
        {
            int restoredHp = (int)Math.Max(1, Math.Ceiling(MyStatus.MaxHp * 0.1));
            MyStatus.Hp = restoredHp;
            MyStatus.Mp = MyStatus.MaxMp;
            IsDead = false;

            Tavern tavern = new Tavern();
            tavern.Start();
        }
        #endregion

        #region 저장된 플레이어 불러오기
        public void Load()
        {
            Status loadedStatus = DataManager.Instance.Load();
            ApplyStatus(loadedStatus);
        }
        #endregion

        #region 이름 설정 & 기본 스텟 설정
        public void SetName(string name)
        {
            MyStatus.Name = name;
            Console.WriteLine($"설정된 이름 : {MyStatus.Name}");
        }
        #endregion

        #region 플레이어 전투

        #region 전투 액션 선택

        /// <summary>
        /// 전투의 액션 선택창 띄우는 함수
        /// </summary>
        /// <returns></returns>
        int SelectBattleAction()
        {
            //콘솔 한줄 지우기 간편화
            const string Esc = "\u001b[";
            //플레이어가 몇번쨰 선택지를 골랐는지
            int selected = 0;
            //키 입력받기 간편화
            ConsoleKey key;
            //첫번째 실행시 2줄이 추가로 나오는 문제를 해결하기 위해 첫번쨰 실행인지 확인하는 변수
            bool firstRun = false;

            //선택메뉴에 띄울 옵션들
            string[] menuOptions = new string[]
            {
                    "공격",
                    "스킬",
                    "아이템",
                    "탐색",
                    "포기"
            };

            //플레이어가 선택지를 선택(Enter)할때까지 반복
            do
            {
                if (firstRun)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Console.Write($"{Esc}2K");
                        Console.Write($"{Esc}1F");
                    }
                }
                else
                {
                    firstRun = true;
                }

                Console.WriteLine();
                Console.Write("|");
                for (int i = 0; i < menuOptions.Length; i++)
                {

                    Console.Write(" ");

                    if (i == selected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(">" + menuOptions[i]);
                    }
                    else
                    {
                        Console.Write(" " + menuOptions[i]);
                    }

                    Console.ResetColor();
                }
                Console.WriteLine(" |");

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.RightArrow)
                {
                    selected = (selected + 1) % menuOptions.Length;
                }
                if (key == ConsoleKey.LeftArrow)
                {
                    selected = (selected - 1 + menuOptions.Length) % menuOptions.Length;
                }
            }
            while (key != ConsoleKey.Enter);

            return selected;
        }
        #endregion

        #region 플레이어가 선택한 액션 실행
        public void PerformAction(Character target)
        {
            battleLogService.ShowBattleInfo(this, target);

            int input = SelectBattleAction();

            switch (input)
            {

                case 0:
                    {
                        Attack(target);
                        break;
                    }
                case 1:
                    {
                        BattleSkillLogic(target);
                        break;
                    }
                case 2:
                    {
                        UseItem();
                        break;
                    }
                case 3:
                    {
                        Console.Clear();
                        Console.WriteLine("탐색 중...");
                        Thread.Sleep(1500);
                        Console.Clear();

                        target.MyStatus.ShowStatus();

                        UiHelper.WaitForInput("[ENTER]를 눌러 계속");

                        PerformAction(target);
                        break;
                    }
                case 4:
                    {
                        BattleRun();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return;
        }
        #endregion

        #region 스킬 사용

        #region 스킬 리스트 표시
        public SkillType ShowSkillList() => skillSystem.ShowCanUseSkill();
        #endregion

        //스킬 클래스에서 스킬을 가져와 사용하는 함수

        void BattleSkillLogic(Character target)
        {
            SkillType useSkill = ShowSkillList();
            if (string.IsNullOrWhiteSpace(useSkill.name))
            {
                PerformAction(target);
                return;
            }
            if (MyStatus.Mp < useSkill.skillMana)
            {
                Console.WriteLine("마나가 부족합니다.");
                UiHelper.WaitForInput("[ENTER]를 눌러 계속");
                PerformAction(target);
                return;
            }

            UseAttackSkill(useSkill);

            switch (useSkill.name)
            {
                case "파이어볼":
                    {
                        StatusEffects.EnemyAddTickSkill(useSkill.name, useSkill.skillTurn);
                        break;
                    }
                case "아쿠아 볼":
                    {
                        StatusEffects.AddTickSkill(useSkill.name, useSkill.skillTurn);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion

        #region 도망치기

        void BattleRun()
        {
            Random random = new Random();
            int chance = random.Next(1, 101);
            if (chance >= 50)
            {
                isbattleRun = true;
                Console.Clear();
                Console.WriteLine("전투에서 탈출했다");

                GameManager.Instance.ReturnToLobby();
                isbattleRun = false;
            }
            //만약 실패할시 플레이어에게 실패 메세지를 띄움
            else
            {
                isbattleRun = false;
                Console.WriteLine("도망치는데 실패했다");
                UiHelper.WaitForInput("[ENTER]를 눌러 계속");
            }
        }

        #endregion

        #endregion 플레이어 전투
    }
}

