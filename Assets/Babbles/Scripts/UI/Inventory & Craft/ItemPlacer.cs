using ClemCAddons;
using ClemCAddons.Player;
using ClemCAddons.Utilities;
using Luminosity.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ItemPlacer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PlacingType _placingType = PlacingType.Free;
    [SerializeField] private float _distance = 5;
    [SerializeField] private float _minimumDistanceOffset = 0;
    [SerializeField, DrawIf("_placingType", PlacingType.Vertical, ComparisonType.GreaterOrEqual)] private float _height;
    [SerializeField] private Transform _topParent = null;
    [SerializeField] private Vector3 _rotationOffset;
    [Header("Miniature")]
    [SerializeField] private float _distanceInFront = 0.5f;
    [SerializeField] private float _arcHeight = 1f;
    [SerializeField] private float _animationSpeed = 2f;
    [SerializeField] private float _animationAcceleration = 1f;
    [SerializeField] private float _growthSpeed = 2f;
    [SerializeField] private float _growthAcceleration = 4f;
    [Header("Preview")]
    [SerializeField] private int _gridSize = 10;
    [SerializeField] private Color _previewColor = Color.white;

    [Header("Grenade")]
    [SerializeField] private bool _isGrenade = false;

    private static int _counter = 0;

    private Item _itemType;
    private Transform _player;
    private bool _lock = false;
    private float _perc = 0f;
    private float _percW = 0.5f;

    public Transform Player
    {
        get
        {
            if (_player == null)
                _player = FindObjectOfType<CharacterMovement>().transform;
            return _player;
        }
        set => _player = value;
    }
    public Item ItemType { get => _itemType; set => _itemType = value; }
    public float Distance { get => _distance; }
    public PlacingType PlacingTypeA { get => _placingType; }
    public float Height { get => _height; }
    public int GridSize { get => _gridSize; }
    public Color PreviewColor { get => _previewColor; }
    public static bool ConstructionMode { get => _counter != 0; }

    public enum PlacingType
    {
        None,
        Free,
        Horizontal,
        Vertical,
        Wall
    }

    void Start()
    {
        FreshBulleAnimation.StartPoseAnimation(FreshBulleAnimation.PoseType.Lifting);

        if(_isGrenade == true)
        {
            AudioManager.Start3DSound("S_MecheGrenade", transform);
        }

        _counter++;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_placingType)
        {
            case PlacingType.Free:
                FreePlacing();
                break;
            case PlacingType.Horizontal:
                HorizontalPlacing();
                break;
            case PlacingType.Vertical:
                VerticalPlacing();
                break;
            case PlacingType.Wall:
                WallPlacing();
                break;
            case PlacingType.None:
                break;
            default:
                break;
        }

        PlaceInFront();

        TurnWithBulle();

        if (!_lock && (InputManager.GetButtonDown("Interaction") || (InputManager.GetButtonDown("UI_Submit") && InputManager.IsUsingController) )) // confirm
        {
            if(_isGrenade == true)
            {
                AudioManager.Instance.StopUniqueSound(ESoundType.REPETITIVE3D, "S_MecheGrenade");
            }

            FreshBulleAnimation.ResetPoseAnimation();
            _ = GameTools.DelayedCall(10, () =>
            {
                _counter--;
                _ = DisableTransparent();
                FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Placing);
                BabblesVibration.CustomVibration(0.15f, 0.3f);
            });

            return; // so as to not move the object
        }
        if (InputManager.GetButtonDown("UI_Cancel")) // cancel
        {
            if (_isGrenade == true)
            {
                AudioManager.Instance.StopUniqueSound(ESoundType.REPETITIVE3D, "S_MecheGrenade");
            }

            FreshBulleAnimation.ResetPoseAnimation();
            _counter--;
            
            InventoryManager.Instance.AddItem(_itemType); // add back item 

            Destroy(_topParent.gameObject); // destroy the parent of parent, according to usable item structure
            return; // so as to not do anything that would cause an error on an object that's being destroyed
        }

    }

    private async Task DisableTransparent()
    {
        gameObject.SetActive(false);
        float howFar = 0;
        Vector3 origin = transform.parent.GetChild(2).localPosition;
        float y = 0;
        float yA;
        while (transform.parent.GetChild(2).localPosition != Vector3.zero)
        {
            if(howFar < 0.5)
            {
                y += (howFar - 0.5f).Abs() * Time.deltaTime * 8 * _animationSpeed;
                yA = Mathf.Lerp(origin.y, _arcHeight, y);
            }
            else
            {
                y -= (howFar - 0.5f).Abs() * Time.deltaTime * 8 * _animationSpeed;
                yA = Mathf.Lerp(0, _arcHeight, y);
            }
            transform.parent.GetChild(2).localPosition = Vector3.Lerp(origin, Vector3.zero, howFar).SetY(yA);
            howFar += Time.deltaTime * _animationSpeed;
            _animationSpeed += _animationAcceleration * Time.deltaTime;
            await Task.Delay((Time.deltaTime * 1000).Round());
        }
        while(transform.parent.GetChild(2).localScale != Vector3.one)
        {
            transform.parent.GetChild(2).localScale = transform.parent.GetChild(2).localScale.ClampedConstantInterpolation(Vector3.one, Time.deltaTime * _growthSpeed);
            _growthSpeed += _growthAcceleration * Time.deltaTime;
            await Task.Delay((Time.deltaTime * 1000).Round());
        }
        transform.parent.GetChild(0).gameObject.SetActive(true);
        transform.parent.GetChild(2).gameObject.SetActive(false);
        BabblesVibration.CustomVibration(0.15f, 0.3f);
        PlacingFeedback(); //Vibration
        // child 0 is never you, you are not the favorite child -> Maybe a better and safer way to desactivate the object
    }

    private void TurnWithBulle()
    {
        var rot = _player.GetComponentInChildren<FollowVelocity>().transform.rotation * Quaternion.Euler(_rotationOffset);
        transform.parent.GetChild(0).rotation = transform.parent.GetChild(1).rotation = transform.parent.GetChild(2).rotation = rot;
    }

    private void PlaceInFront()
    {
        if (transform.parent.childCount < 3)
        {
            Debug.LogError("Missing BU_Miniature: a clear version of the miniaturized item");
            return;
        }
        transform.parent.GetChild(2).gameObject.SetActive(true);
        transform.parent.GetChild(2).position = _player.position + _player.GetComponentInChildren<FollowVelocity>().transform.forward * _distanceInFront;
    }


    private void PlaceAt(Vector3 position, bool air = false)
    {
        if (!air)
        {
            position.y += 1.5f;
            position.y -= GameTools.FindGround(position, 0, 3, Player.GetComponent<CharacterMovement>().CollisionLayer, 2f);
        }
        _topParent.position = position;
    }
    private void FreePlacing()
    {
        Vector3 pos = transform.position;
        var ray = Camera.allCameras[0].ScreenPointToRay(InputManager.mousePosition);
        var r = ray.origin.CastToLineOnly(ray.origin + ray.direction * 10, Player.GetComponent<CharacterMovement>().CollisionLayer, out var hit);
        if (r)
        {
            pos = hit.point;
        }
        if (pos.Distance(Player.position) > _distance)
        {
            pos = Player.position + (pos - Player.position).ClampAround0(_distance);
        }
        PlaceAt(pos);
    }

    private void HorizontalPlacing()
    {
        Vector3 minPos = Player.position + (Player.GetComponentInChildren<FollowVelocity>().transform.forward * (Player.lossyScale.x + transform.lossyScale.x + _minimumDistanceOffset));

        Vector3 maxPos = Player.position + (Player.GetComponentInChildren<FollowVelocity>().transform.forward * _distance);

        Vector3 pos = Vector3.Lerp(minPos, maxPos, _perc);

        var r = Player.position.CastToLineOnly(pos + Vector3.zero.SetY(1.5f) + Player.position.Direction(pos) * transform.lossyScale.x / 2, Player.GetComponent<CharacterMovement>().CollisionLayer, out var hit);
        if (r)
        {
            pos = Player.position + Player.position.Direction(pos) * (hit.distance - transform.lossyScale.x / 2);
        }

        if (InputManager.IsUsingController)
        {
            if (InputManager.GetAxis("PlacingVertical") != 0)
            {
                _perc = (_perc + InputManager.GetAxis("PlacingVertical") * Time.deltaTime * 2).Clamp01();
            }
        }
        else
        {
            if (InputManager.mouseScrollDelta.y != 0)
            {
                _perc = (_perc + InputManager.mouseScrollDelta.y * Time.deltaTime * 2).Clamp01();
            }
        }
        PlaceAt(pos);
    }
    private void VerticalPlacing()
    {
        float distance2D = _distance;
        var t = Player.position.CastToLineOnly(Player.position + Player.GetComponentInChildren<FollowVelocity>().transform.forward * _distance, Player.GetComponent<CharacterMovement>().CollisionLayer, out var c);
        if (t)
        {
            distance2D = c.distance - transform.lossyScale.x / 2;
        }
        var anchor = Player.position + Player.GetComponentInChildren<FollowVelocity>().transform.forward * distance2D;
        anchor.y += 1.5f;
        anchor.y -= GameTools.FindGround(anchor, 0, 3, Player.GetComponent<CharacterMovement>().CollisionLayer);

        Vector3 minPos = anchor + Vector3.up * (transform.lossyScale.y + _minimumDistanceOffset);
        Vector3 maxPos = anchor + Vector3.up * _height;

        Vector3 pos = Vector3.Lerp(minPos, maxPos, _perc);

        var r = minPos.CastToLineOnly(pos, Player.GetComponent<CharacterMovement>().CollisionLayer, out var hit);
        if (r)
        {
            pos = Player.position + minPos + Vector3.up * (hit.distance - transform.lossyScale.y / 2);
        }

        if (InputManager.IsUsingController)
        {
            if (InputManager.GetAxis("PlacingVertical") != 0)
            {
                _perc = (_perc + InputManager.GetAxis("PlacingVertical") * Time.deltaTime * 2).Clamp01();
            }
        }
        else
        {
            if (InputManager.mouseScrollDelta.y != 0)
            {
                _perc = (_perc + InputManager.mouseScrollDelta.y * Time.deltaTime * 2).Clamp01();
            }
        }
        PlaceAt(pos, true);
    }
    private void WallPlacing()
    {
        Vector3 anchor = Player.position + Camera.allCameras[0].transform.right * (_percW - 0.5f) * _height;

        Vector3 minPos = anchor + Vector3.up * (1 + _minimumDistanceOffset);
        Vector3 maxPos = anchor + Vector3.up * _height;


        Vector3 pos = Vector3.Lerp(minPos, maxPos, _perc);

        var r = pos.CastToLineOnly(pos + Camera.allCameras[0].transform.forward * (1 + transform.lossyScale.x / 2) * _distance, Player.GetComponent<CharacterMovement>().CollisionLayer, out var hit);
        if (r)
        {
            pos = pos + Camera.allCameras[0].transform.forward * (hit.distance - transform.lossyScale.x / 2);

            var t = GameTools.FindGround(pos + Vector3.zero.SetY(1.5f), 0, 3, Player.GetComponent<CharacterMovement>().CollisionLayer);
            if (t == 3 || t == 0)
            {
                PlaceAt(Player.position + Vector3.zero.SetY(1.5f + transform.lossyScale.y));
            }
            PlaceAt(pos, true);
        }
        else
        {
            PlaceAt(Player.position + Vector3.zero.SetY(1.5f + transform.lossyScale.y));
        }

        if (InputManager.IsUsingController)
        {
            if (InputManager.GetAxis("PlacingVertical") != 0)
            {
                _perc = (_perc + InputManager.GetAxis("PlacingVertical") * Time.deltaTime * 2).Clamp01();
            }
            if (InputManager.GetAxis("PlacingHorizontal") != 0)
            {
                _percW = (_percW + InputManager.GetAxis("PlacingHorizontal") * Time.deltaTime * 2).Clamp01();
            }
        }
        else
        {
            if (InputManager.mouseScrollDelta.y != 0)
            {
                _perc = (_perc + InputManager.mouseScrollDelta.y * Time.deltaTime * 2).Clamp01();
            }
            var sign = (InputManager.mousePosition.x - Camera.allCameras[0].WorldToScreenPoint(pos).x).Sign();
            _percW = (_percW + sign * Time.deltaTime * 2).Clamp01();
        }

    }

    private void PlacingFeedback()
    {
        BabblesVibration.CustomVibration(0.15f, 0.3f);  //Vibration
        AudioManager.Start2DSoundSingleCall("S_PlacementObjet", 400);
    }

}

#if(UNITY_EDITOR)
[CustomEditor(typeof(ItemPlacer))]
public class ItemPlacerEditor : Editor
{
    private float gridSize = 10f;
    private float unitSize { get => 1/gridSize; }
    private float unitPixelSize;
    private Color previewColor;
    private ItemPlacer.PlacingType placingType;
    private ItemPlacer placer;
    private Texture bulleTexture;
    private int captionID = 0;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        placer = target as ItemPlacer;
        if(bulleTexture == null)
        {
            var r = Resources.Load<Sprite>("Images/Debug/bulleImage");
            if (r != null)
                bulleTexture = r.texture;
            else
                Debug.LogError("Missing bulle's texture, who deleted it? J'accuse comme Emile Zola");
        }
        // INITIALIZE VALUES
        captionID = 0;
        gridSize = placer.GridSize;
        previewColor = placer.PreviewColor;
        placingType = placer.PlacingTypeA;
        // DRAW PREVIEW
        float size = Screen.width - 35;
        Rect rect = EditorGUILayout.GetControlRect(false, size);
        unitPixelSize = unitSize * rect.size.x;
        Vector2 bulleSize = FindObjectOfType<CharacterMovement>().GetComponent<Collider>().bounds.extents * 2;
        bulleSize = bulleSize * unitPixelSize;
        Vector2 bullePos = new Vector2(rect.center.x - (bulleSize.x / 2f), rect.y + rect.size.y - bulleSize.y);
        DrawRectangle(size, size, rect);
        DrawRectangle(rect.center.x-0.5f, rect.y, 1, rect.size.y, rect, Color.red);
        DrawRectangle(rect.xMin, bullePos.y + bulleSize.y/2, rect.size.x / 2, 1, rect, Color.red);
        switch (placingType)
        {
            case ItemPlacer.PlacingType.Free:
                DrawFree(rect, bullePos, bulleSize, placer.Distance);
                break;
            case ItemPlacer.PlacingType.Horizontal:
                DrawHorizontal(rect, bullePos, bulleSize, placer.Distance);
                break;
            case ItemPlacer.PlacingType.Vertical:
                DrawVertical(rect, bullePos, bulleSize, placer.Distance, placer.Height);
                break;
            case ItemPlacer.PlacingType.Wall:
                DrawWall(rect, bullePos, bulleSize, placer.Distance, placer.Height);
                break;
            case ItemPlacer.PlacingType.None:
                break;
            default:
                break;
        }
        DrawPlayer(rect, bullePos, bulleSize);
        DrawCaption(rect);
        // DRAW INPUT CAPTION NEXT (TOP RIGHT)
    }

    private void DrawCaption(Rect rect)
    {
        DrawTitle(rect, "Captions");
        DrawCaption(rect, "Player's up and forward directions", Color.red);
        if(placingType == ItemPlacer.PlacingType.Wall)
        {
            DrawCaption(rect, "Furthest reachable wall", new Color(0.4f, 0.4f, 0.4f));
            DrawCaption(rect, "Position when no wall is at proximity", Color.black);
        }
        DrawTitle(rect, "Inputs");
        switch (placingType)
        {
            case ItemPlacer.PlacingType.Free:
                DrawCaption(rect, "Look: Move");
                DrawCaption(rect, "Distance: max distance", textSize: 10);
                break;
            case ItemPlacer.PlacingType.Horizontal:
                DrawCaption(rect, "Scroll / DPAD: Slide closer / further");
                DrawCaption(rect, "Distance: max sliding", textSize: 10);
                break;
            case ItemPlacer.PlacingType.Vertical:
                DrawCaption(rect, "Scroll / DPAD: Slide lower / higher");
                DrawCaption(rect, "Distance 1: distance from player", textSize: 10);
                DrawCaption(rect, "Distance 2: max sliding height", textSize: 10);
                break;
            case ItemPlacer.PlacingType.Wall:
                DrawCaption(rect, "Look: Move up/down on wall");
                DrawCaption(rect, "Scroll / DPAD: Slide left / right\n(second distance value)");
                DrawCaption(rect, "Distance 1: max distance from player", textSize: 10);
                DrawCaption(rect, "Distance 2: max sliding", textSize: 10);
                break;
            default:
                break;
        }
    }

    private void DrawCaption(Rect rect, string text, Color color = default, int textSize = 12, float spacing = 1)
    {
        var style = GUI.skin.label;
        if (color == default)
        {
            rect.x = rect.xMin + unitPixelSize * 0.25f;
            rect.y = rect.yMin + unitPixelSize * 0.25f * (captionID + 1) + captionID * spacing * unitPixelSize * 0.25f - 2.5f;
            rect.height = unitPixelSize * 0.25f + 5;
            rect.width = unitPixelSize;
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Overflow;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            style.fontSize = textSize;
            EditorGUI.LabelField(rect, text, new GUIStyle(style));
            captionID++;
            return;
        }
        rect.x = rect.xMin + unitPixelSize * 0.25f;
        rect.y = rect.yMin + unitPixelSize * 0.25f * (captionID+1) + captionID * spacing * unitPixelSize * 0.25f;
        rect.width = unitPixelSize * 0.25f;
        rect.height = unitPixelSize * 0.25f;
        EditorGUI.DrawRect(rect, color);
        rect.x += rect.width * 1.5f;
        rect.width = unitPixelSize;
        rect.height += 5;
        rect.y -= 2.5f;
        style.alignment = TextAnchor.MiddleLeft;
        style.clipping = TextClipping.Overflow;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.fontSize = textSize;
        EditorGUI.LabelField(rect, text, new GUIStyle(style));
        captionID++;
    }
    private void DrawTitle(Rect rect, string text, int textSize = 15, float spacing = 1)
    {
        if(captionID != 0)
            captionID++;
        rect.x = rect.xMin + unitPixelSize * 0.25f;
        rect.y = rect.yMin + unitPixelSize * 0.25f * (captionID + 1) + captionID * spacing * unitPixelSize * 0.25f - 2.5f;
        rect.height = unitPixelSize * 0.25f + 5;
        rect.width = unitPixelSize;
        var style = GUI.skin.label;
        style.alignment = TextAnchor.MiddleLeft;
        style.clipping = TextClipping.Overflow;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.fontSize = textSize;
        EditorGUI.LabelField(rect, text, new GUIStyle(style));
        captionID++;
    }

    private void DrawPlayer(Rect rect, Vector2 pos, Vector2 size)
    {
        rect.position = pos;
        rect.size = size;
        GUI.DrawTexture(rect, bulleTexture, ScaleMode.ScaleAndCrop);    }

    private void DrawHorizontal(Rect rect, Vector2 bullePos, Vector2 bulleSize, float distance)
    {
        rect.position = rect.position.SetX(rect.center.x - distance * unitPixelSize - bulleSize.x/2);
        rect.position = rect.position.SetY(bullePos.y + bulleSize.y * 0.375f);
        rect.size = new Vector2(distance * unitPixelSize, bulleSize.y * 0.25f);
        EditorGUI.DrawRect(rect, previewColor);
    }
    private void DrawVertical(Rect rect, Vector2 bullePos, Vector2 bulleSize, float distance, float height)
    {
        rect.position = rect.position.SetX(rect.center.x - distance * unitPixelSize - bulleSize.x / 2);
        rect.position = rect.position.SetY(bullePos.y + bulleSize.y * 0.375f - height * unitPixelSize);
        rect.size = new Vector2(bulleSize.y * 0.25f, height * unitPixelSize);
        EditorGUI.DrawRect(rect, previewColor);
    }
    private void DrawWall(Rect rect, Vector2 bullePos, Vector2 bulleSize, float distance, float height)
    {
        var xmin = rect.xMin;
        DrawRectangle(rect.center.x - unitPixelSize * 0.5f, bullePos.y - (1.5f + 0.75f * 0.5f) * unitPixelSize, unitPixelSize, unitPixelSize * 0.75f, rect, Color.black);
        rect.position = rect.position.SetX(rect.center.x - distance * unitPixelSize - bulleSize.x / 2);
        rect.position = rect.position.SetY(bullePos.y + bulleSize.y * 0.375f - distance * unitPixelSize);
        rect.size = new Vector2(bulleSize.y * 0.25f, distance * unitPixelSize);
        EditorGUI.DrawRect(rect, previewColor);
        rect.size = rect.size.SetX(rect.position.x - xmin);
        rect.position = rect.position.SetX(xmin);
        EditorGUI.DrawRect(rect, new Color(0.4f,0.4f,0.4f));
    }
    private void DrawFree(Rect rect, Vector2 bullePos, Vector2 bulleSize, float distance)
    {
        DrawRectangle(bullePos.x + bulleSize.x, bullePos.y + bulleSize.y * 0.375f, distance * unitPixelSize, bulleSize.y * 0.25f, rect, previewColor);
        rect.position = rect.position.SetX(rect.center.x - distance * unitPixelSize - bulleSize.x / 2);
        rect.position = rect.position.SetY(bullePos.y + bulleSize.y * 0.375f);
        rect.size = new Vector2(distance * unitPixelSize, bulleSize.y * 0.25f);
        EditorGUI.DrawRect(rect, previewColor);
    }

    private void DrawRectangle(float i_height, float i_width, Rect rect)
    {
        rect.height = i_height;
        rect.width = i_width;

        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

    }

    // Used when I don't want to edit the rect of the current scope, as it is less readable
    private void DrawRectangle(float i_x, float i_y, float i_width, float i_height, Rect rect, Color color)
    {
        rect.x = i_x;
        rect.y = i_y;
        rect.height = i_height;
        rect.width = i_width;

        EditorGUI.DrawRect(rect, color);

    }
}
#endif