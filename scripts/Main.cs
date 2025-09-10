using Godot;

namespace RainerCat.scripts;

public partial class Main : Node2D
{
    private Node _actorNode;

    private Vector2I _clickStartPos = Vector2I.Zero;
    private Vector2I _windowStartPos = Vector2I.Zero;
    private bool _isDragging;
    private bool _isFrame1Selected;
    private Timer _resetTimer;
    private int _currentFrame;
    private AnimatedSprite2D _myCat;
    private Node _globalInput;
    [Export] public bool EnableDrag { get; set; } = true;
    [Export] public int KeyPressCount { get; set; }
    [Export] public float ResetDelay { get; set; } = 0.07f; 

    public override void _Ready()
    {
        base._Ready();
        GetTree().Root.SetTransparentBackground(true);

        BindNodes();
    }

    private void ResetTimerOnTimeout()
    {
        _currentFrame = 0;
        _myCat.SetFrame(_currentFrame);
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseButton)
        {
            if (mouseButton.Pressed)
            {
                _clickStartPos = DisplayServer.MouseGetPosition();
                _windowStartPos = GetWindow().Position;
                _isDragging = true;
                
                ChangeAnimateFrame(_isFrame1Selected ? 1 : 2);
                _isFrame1Selected = !_isFrame1Selected; 
                _resetTimer.Start(ResetDelay);
            }
            else
            {
                _isDragging = false;
            }
        }
    }

    private void OnKeyPressedNamed(int keyCode, string keyName)
    {
        ChangeAnimateFrame(_isFrame1Selected ? 1 : 2);
        _isFrame1Selected = !_isFrame1Selected; 
        
        _resetTimer.Start(ResetDelay);
    }

    private void ChangeAnimateFrame(int frame)
    {
        _currentFrame = frame;
        _myCat.SetFrame(_currentFrame);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_isDragging && EnableDrag)
        {
            var mousePos = DisplayServer.MouseGetPosition();
            var distance = mousePos - _clickStartPos;
            GetWindow().SetPosition(_windowStartPos + distance);
        }
    }

    private void BindNodes()
    {
        _actorNode = GetNode<Node>("Actor");
        _resetTimer = GetNode<Timer>("ResetTimer");
        _myCat = GetNode<AnimatedSprite2D>("Actor/AnimatedSprite2D");
        _globalInput = GetNode<Node>("GlobalInput");
        var actorArea2D = GetNode<Area2D>("Actor/Area2D");
        
        actorArea2D.InputEvent += OnInputEvent;

        _resetTimer.OneShot = true;
        _resetTimer.Timeout += ResetTimerOnTimeout;
        
        _globalInput.Connect("key_pressed_named", Callable.From<int, string>(OnKeyPressedNamed));
    }
}