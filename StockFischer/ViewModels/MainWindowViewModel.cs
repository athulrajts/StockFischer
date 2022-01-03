﻿using StockFischer.Models;
using OpenPGN;
using OpenPGN.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using System.Windows.Input;

namespace StockFischer.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        [Reactive]
        public LiveBoard Board { get; set; }

        [Reactive]
        public Color Perspective { get; set; } = Color.White;

        [Reactive]
        public bool IsPromoting { get; set; }

        public string InputFen { get; set; }

        public ICommand LoadFenCommand { get; set; }
        public ICommand TogglePerspectiveCommand { get; set; }

        public MainWindowViewModel()
        {
            //Board = LiveBoard.NewGame();
            Board = LiveBoard.FromPgnFile(@"C:\Users\Athul\Desktop\pgn.txt");
            //Board.MovePlayed += OnMove;

            //this.WhenAnyValue(x => x.Board)
            //    .Buffer(2, 1)
            //    .Select(b => (Previous: b[0], Current: b[1]))
            //    .Subscribe(OnBoardChanged);

            LoadFenCommand = ReactiveCommand.Create(LoadFen);
            TogglePerspectiveCommand = ReactiveCommand.Create(TogglePerspective);
        }

        private void LoadFen() => Board = LiveBoard.FromFen(InputFen);
        private void TogglePerspective() => Perspective = Perspective.Invert();
    }
}
