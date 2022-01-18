﻿using Lichess;
using Microsoft.Extensions.Logging;
using OpenPGN.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using StockFischer.Engine;
using StockFischer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace StockFischer.Liches;

public class LiveGameModel : ReactiveObject
{
    private readonly ILogger _logger;
    private readonly LiveGameStream _stream;
    private readonly Game _game;
    private readonly List<string> _moves;
    private static ExportGameQueryParameters parameters = new() { Clocks = false, Evals = false, Literate = false};
    private readonly IObservable<long> _timer;

    [Reactive]
    public LiveBoard Board { get; set; }

    [Reactive]
    public TimeSpan WhiteTimeRemaining { get; set; } = TimeSpan.Zero;

    [Reactive]
    public TimeSpan BlackTimeRemaining { get; set; } = TimeSpan.Zero;

    public LiveGameModel(string gameId, ILogger logger)
    {
        _logger = logger;
        _stream = new LiveGameStream(gameId);
        _game = Game.FromPgn(new ExportGameRequest(gameId, parameters).GetAsync().Result);
        _stream.MovePlayed += OnMovePlayed;

        Board = LiveBoard.FromGame(_game);
        _moves = Board.Moves.AsUciMoves().ToList();
        _stream.StartStream();

        _timer = Observable.Interval(TimeSpan.FromSeconds(1));
        _timer.Subscribe(Elapsed);
    }

    private void Elapsed(long obj)
    {
        if(Board.ActiveColor == Color.White)
        {
            WhiteTimeRemaining -= TimeSpan.FromSeconds(1); 
        }
        else
        {
            BlackTimeRemaining -= TimeSpan.FromSeconds(1);
        }
    }

    private void OnMovePlayed(object sender, GameMove e)
    {
        WhiteTimeRemaining = TimeSpan.FromSeconds(e.WhiteTimeRemaining);
        BlackTimeRemaining = TimeSpan.FromSeconds(e.BlackTimeRemaining);

        _logger.LogDebug("Lichess Move recieved : {move}", e.LastMove);

        if(!_moves.Contains(e.LastMove) && UCIMove.Parse(e.LastMove) is { } move)
        {
            Application.Current.Dispatcher.Invoke(() => Board.TryMakeMove(move.From, move.To));
        }
    }
}
