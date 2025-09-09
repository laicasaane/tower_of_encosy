using System;
using EncosyTower.Annotations;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.Input;
using EncosyTower.VisualDebugging.Commands;

namespace EncosyTower.Samples.VisualDebugging
{
    internal enum ResourceType
    {
        None,
        Gold,
        Crystal,
    }

    [Label("Resource", "Resources"), VisualOrder(0)]
    internal sealed partial class VisualCommand_Resource : IVisualCommand, IObservableObject
    {
        [ObservableProperty]
        private ResourceType Type
        {
            get => Get_Type();
            set => Set_Type(value);
        }

        [ObservableProperty]
        private int Amount
        {
            get => Get_Amount();
            set => Set_Amount(value);
        }

        public void Execute()
        {
            RuntimeLoggerAPI.LogInfoSlim($"Resource: Type={Type} Amount={Amount}");
        }

        [RelayCommand]
        private void SetType(Enum value)
        {
            _type = (ResourceType)value;
        }

        [RelayCommand]
        private void SetAmount(int value)
        {
            _amount = value;
        }
    }

    [Label("Gold", "Resources"), VisualOrder(1)]
    internal sealed partial class VisualCommand_Gold : IVisualCommand, IObservableObject
    {
        [ObservableProperty]
        private int Amount
        {
            get => Get_Amount();
            set => Set_Amount(value);
        }

        public void Execute()
        {
            RuntimeLoggerAPI.LogInfoSlim($"Gold: Amount={Amount}");
        }

        [RelayCommand]
        private void SetAmount(int value)
        {
            _amount = value;
        }
    }

    [Label("Crystal", "Resources"), VisualOrder(2)]
    internal sealed partial class VisualCommand_Crystal : IVisualCommand, IObservableObject
    {
        [ObservableProperty]
        private int Amount
        {
            get => Get_Amount();
            set => Set_Amount(value);
        }

        public void Execute()
        {
            RuntimeLoggerAPI.LogInfoSlim($"Crystal: Amount={Amount}");
        }

        [RelayCommand]
        private void SetAmount(int value)
        {
            _amount = value;
        }
    }

    [Label("Level", "Levels")]
    internal sealed partial class VisualCommand_Level : IVisualCommand, IObservableObject
    {
        public VisualCommand_Level()
        {
            _currentLevel = 10;
            _receivedStarterPack = true;
        }

        [ObservableProperty]
        private bool ReceivedStarterPack
        {
            get => Get_ReceivedStarterPack();
            set => Set_ReceivedStarterPack(value);
        }

        [ObservableProperty]
        private int CurrentLevel
        {
            get => Get_CurrentLevel();
            set => Set_CurrentLevel(value);
        }

        public void Execute()
        {
            RuntimeLoggerAPI.LogInfoSlim(
                $"Level: CurrentLevel={CurrentLevel} ReceivedStarterPack={ReceivedStarterPack}"
            );
        }

        [RelayCommand]
        private void SetReceivedStarterPack(bool value)
        {
            _receivedStarterPack = value;
        }

        [RelayCommand]
        private void SetCurrentLevel(int value)
        {
            _currentLevel = value;
        }
    }

    [Label("Name", "Player")]
    internal sealed partial class VisualCommand_PlayerName : IVisualCommand, IObservableObject, IInitializable
    {
        [ObservableProperty]
        [VisualOptions(nameof(NameOptions), true)]
        private string Name
        {
            get => Get_Name();
            set => Set_Name(value);
        }

        private string[] NameOptions => new string[] { "Alice", "Bob", "Charlie", "Dave" };

        public void Initialize()
        {
            Name = "Alice";
        }

        public void Execute()
        {
            RuntimeLoggerAPI.LogInfoSlim($"Player Name: {Name}");
        }

        [RelayCommand]
        private void SetName(string value)
        {
            Name = value;
        }

        [RelayCommand]
        private void SetOptionForName(VisualOption option)
        {
            Name = option.Value;
        }
    }
}
