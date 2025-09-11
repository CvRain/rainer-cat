using Godot;

namespace RainerCat.scripts;

public partial class Main : Node2D
{
    private Node _actorNode;
    private Node2D _userCat;

    private Vector2I _clickStartPos = Vector2I.Zero;
    private Vector2 _userCatStartPos = Vector2.Zero;
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
        
        var screenSize = DisplayServer.ScreenGetSize();
        GetTree().GetRoot().SetSize(screenSize);

        // var window = GetWindow();
        // window.MousePassthrough = true;
        // window.MousePassthroughPolygon = [new Vector2(1920, 1080)];

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
                _userCatStartPos = _userCat.Position;
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
            _userCat.Position = _userCatStartPos + distance;
        }
    }

    private void BindNodes()
    {
        _userCat = GetNode<Node2D>("UserCat");
        _actorNode = GetNode<Node>("UserCat/Actor");
        _resetTimer = GetNode<Timer>("ResetTimer");
        _myCat = GetNode<AnimatedSprite2D>("UserCat/Actor/AnimatedSprite2D");
        _globalInput = GetNode<Node>("GlobalInput");
        var actorArea2D = GetNode<Area2D>("UserCat/Actor/Area2D");
        
        actorArea2D.InputEvent += OnInputEvent;

        _resetTimer.OneShot = true;
        _resetTimer.Timeout += ResetTimerOnTimeout;
        
        _globalInput.Connect("key_pressed_named", Callable.From<int, string>(OnKeyPressedNamed));
    }
}