namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using DevelopersHub.RealtimeNetworking.Client;
    using UnityEngine.Windows;

    public class CameraController : MonoBehaviour
    {

        private static CameraController _instance = null; public static CameraController instance { get { return _instance; } }

        [SerializeField] private Camera _camera = null;
        [SerializeField] private float _moveSmooth = 5;
        [SerializeField] private float _mouseZoomSpeed = 10;

        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _zoomSmooth = 5;

        [HideInInspector] public Controls inputs = null;

        private bool _zooming = false;
        private bool _moving = false;
        [SerializeField] private float _zoom = 5; public float zoomScale { get { return _camera.orthographicSize / _zoomMax; } }
        [SerializeField] private float _zoomMax = 10;
        [SerializeField] private float _zoomMin = 1;
        private Vector2 _zoomPositionOnScreen = Vector2.zero;
        private Vector3 _zoomPositionInWorld = Vector3.zero;
        private float _zoomBaseValue = 0;
        private float _zoomBaseDistance = 0;

        private Vector3 _moveRootBasePosition = Vector3.zero;
        private Vector3 _moveInputBaseWorldPosition = Vector3.zero;
        private Vector2 _moveInputBaseScreenPosition = Vector2.zero;
        private Vector2 _moveBaseDirection = Vector2.zero;

        private Transform _root = null;
        private Transform _pivot = null;
        private Transform _target = null;

        private bool _building = false; public bool isPlacingBuilding { get { return _building; } set { _building = value; } }
        private Vector3 _buildBasePosition = Vector3.zero;
        private bool _movingBuilding = false;

        private bool _replacing = false; public bool isReplacingBuilding { get { return _replacing; } set { _replacing = value; } }
        private Vector3 _replaceBasePosition = Vector3.zero;
        private bool _replacingBuilding = false;

        [HideInInspector] public Vector3 planDownLeft = Vector3.zero;
        [HideInInspector] public Vector3 planTopRight = Vector3.zero;

        private void Awake()
        {
            _instance = this;
            inputs = new Controls();
            _root = new GameObject("CameraHelper").transform;
            _pivot = new GameObject("CameraPivot").transform;
            _target = new GameObject("CameraTarget").transform;
            _camera.orthographic = true;
            _camera.nearClipPlane = 0;
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 100f;
            _zooming = false;
            _moving = false;
            _pivot.SetParent(_root);
            _target.SetParent(_pivot);
            _root.position = new Vector3(UI_Main.instance._grid.transform.position.x, UI_Main.instance._grid.down + ((UI_Main.instance._grid.up - UI_Main.instance._grid.down) / 2f), UI_Main.instance._grid.transform.position.z);
            _root.eulerAngles = new Vector3(0, 0, 0);
            _pivot.localPosition = Vector3.zero;
            _pivot.localEulerAngles = Vector3.zero;
            _target.localPosition = new Vector3(0, 0, -50);
            _target.localEulerAngles = Vector3.zero;
            AdjustBounds();
            _camera.transform.position = _root.position;
            _camera.orthographicSize = _zoom;
        }

        private void OnEnable()
        {
            inputs.Enable();
            inputs.Main.Move.started += _ => MoveStarted();
            inputs.Main.Move.canceled += _ => MoveCanceled();
            inputs.Main.TouchZoom.started += _ => ZoomStarted();
            inputs.Main.TouchZoom.canceled += _ => ZoomCanceled();
            inputs.Main.PointerClick.performed += _ => ScreenClicked();
        }

        private void OnDisable()
        {
            inputs.Main.Move.started -= _ => MoveStarted();
            inputs.Main.Move.canceled -= _ => MoveCanceled();
            inputs.Main.TouchZoom.started -= _ => ZoomStarted();
            inputs.Main.TouchZoom.canceled -= _ => ZoomCanceled();
            inputs.Main.PointerClick.performed -= _ => ScreenClicked();
            inputs.Disable();
        }

        private void ScreenClicked()
        {
            Vector2 position = inputs.Main.PointerPosition.ReadValue<Vector2>();
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = position;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            if (UI_Main.instance.isActive)
            {
                if (results.Count <= 0)
                {
                    bool found = false;
                    Vector3 planePosition = CameraScreenPositionToWorldPosition(position);
                    Vector3Int gridPos = UI_Main.instance._grid.grid.WorldToCell(planePosition);
                    for (int i = 0; i < UI_Main.instance._grid.buildings.Count; i++)
                    {
                        if (UI_Main.instance._grid.IsGridPositionIsOnBuilding(new Vector2Int(gridPos.x, gridPos.y), UI_Main.instance._grid.buildings[i].currentX, UI_Main.instance._grid.buildings[i].currentY, UI_Main.instance._grid.buildings[i].rows, UI_Main.instance._grid.buildings[i].columns))
                        {
                            found = true;
                            UI_Main.instance._grid.buildings[i].Selected();
                            break;
                        }
                    }
                    if (!found)
                    {
                        if (Building.selectedinstance != null)
                        {
                            Building.selectedinstance.Deselected();
                        }
                    }
                }
                else
                {
                    if (Building.selectedinstance != null)
                    {
                        bool handled = false;
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (results[i].gameObject == UI_BuildingOptions.instance.infoButton.gameObject)
                            {
                                handled = true;
                                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                                UI_Info.instance.OpenBuildingInfo(Building.selectedinstance.data.id, Building.selectedinstance.data.level);
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.upgradeButton.gameObject)
                            {
                                handled = true;
                                UI_BuildingUpgrade.instance.Open();
                                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.instantButton.gameObject)
                            {
                                handled = true;
                                int cost = Data.GetInstantBuildRequiredGems((int)(Building.selectedinstance.data.constructionTime - Player.instance.data.nowTime).TotalSeconds);
                                if (cost <= Player.instance.data.gems)
                                {
                                    SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                                    Packet packet = new Packet();
                                    packet.Write((int)Player.RequestsID.INSTANTBUILD);
                                    Building.selectedinstance.isCons = false;
                                    Building.selectedinstance.level = Building.selectedinstance.level + 1;
                                    Building.selectedinstance.AdjustUI(true);
                                    Building.selectedinstance.lastChange = System.DateTime.Now;
                                    packet.Write(Building.selectedinstance.data.databaseID);
                                    Sender.TCP_Send(packet);
                                } 
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.trainButton.gameObject)
                            {
                                handled = true;
                                UI_Train.instance.Open();
                                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.clanButton.gameObject)
                            {
                                handled = true;
                                UI_Clan.instance.Open();
                                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.spellButton.gameObject)
                            {
                                handled = true;
                                UI_Spell.instance.SetStatus(true);
                                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.researchButton.gameObject)
                            {
                                handled = true;
                                UI_Research.instance.SetStatus(true);
                                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.removeButton.gameObject)
                            {
                                handled = true;
                                Packet packet = new Packet();
                                packet.Write((int)Player.RequestsID.UPGRADE);
                                packet.Write(Building.selectedinstance.data.databaseID);
                                Sender.TCP_Send(packet);
                                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                            }
                            else if (results[i].gameObject == UI_BuildingOptions.instance.boostButton.gameObject)
                            {
                                handled = true;
                                if (UI_BuildingOptions.instance.canDo)
                                {
                                    Packet packet = new Packet();
                                    packet.Write((int)Player.RequestsID.BOOST);
                                    packet.Write(Building.selectedinstance.data.databaseID);
                                    Sender.TCP_Send(packet);
                                    SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                                }
                            }
                            if (handled)
                            {
                                break;
                            }
                        }
                        Building.selectedinstance.Deselected();
                    }
                }
            }
            else if (UI_Battle.instance.isActive)
            {
                if (results.Count <= 0 && (UI_Battle.instance.selectedUnit >= 0 || UI_Battle.instance.selectedSpell >= 0))
                {
                    Vector3 planePosition = CameraScreenPositionToWorldPosition(position);
                    Vector3Int gridPos = UI_Main.instance._grid.grid.WorldToCell(planePosition);
                    if (gridPos.x >= (0 - Data.battleGridOffset) && gridPos.x < (Data.gridSize + Data.battleGridOffset) && gridPos.y >= (0 - Data.battleGridOffset) && gridPos.y < (Data.gridSize + Data.battleGridOffset))
                    {
                        UI_Battle.instance.PlaceUnit(Mathf.FloorToInt(gridPos.x), Mathf.FloorToInt(gridPos.y));
                    }
                }
            }
            else if (UI_WarLayout.instance.isActive)
            {
                if (results.Count <= 0)
                {
                    bool found = false;
                    Vector3 planePosition = CameraScreenPositionToWorldPosition(position);
                    Vector3Int gridPos = UI_Main.instance._grid.grid.WorldToCell(planePosition);
                    for (int i = 0; i < UI_Main.instance._grid.buildings.Count; i++)
                    {
                        if (UI_Main.instance._grid.IsGridPositionIsOnBuilding(new Vector2Int(gridPos.x, gridPos.y), UI_Main.instance._grid.buildings[i].currentX, UI_Main.instance._grid.buildings[i].currentY, UI_Main.instance._grid.buildings[i].rows, UI_Main.instance._grid.buildings[i].columns))
                        {
                            found = true;
                            UI_Main.instance._grid.buildings[i].Selected();
                            break;
                        }
                    }
                    if (!found)
                    {
                        if (Building.selectedinstance != null)
                        {
                            Building.selectedinstance.Deselected();
                        }
                    }
                }
                else
                {
                    if (Building.selectedinstance != null)
                    {
                        Building.selectedinstance.Deselected();
                    }
                }
            }
            else if (UI_Scout.instance.isActive)
            {
                if (results.Count <= 0)
                {
                    bool found = false;
                    Vector3 planePosition = CameraScreenPositionToWorldPosition(position);
                    Vector3Int gridPos = UI_Main.instance._grid.grid.WorldToCell(planePosition);
                    for (int i = 0; i < UI_Main.instance._grid.buildings.Count; i++)
                    {
                        if (UI_Main.instance._grid.IsGridPositionIsOnBuilding(new Vector2Int(gridPos.x, gridPos.y), UI_Main.instance._grid.buildings[i].currentX, UI_Main.instance._grid.buildings[i].currentY, UI_Main.instance._grid.buildings[i].rows, UI_Main.instance._grid.buildings[i].columns))
                        {
                            found = true;
                            UI_Main.instance._grid.buildings[i].Selected();
                            break;
                        }
                    }
                    if (!found)
                    {
                        if (Building.selectedinstance != null)
                        {
                            Building.selectedinstance.Deselected();
                        }
                    }
                }
                else
                {
                    if (Building.selectedinstance != null)
                    {
                        Building.selectedinstance.Deselected();
                    }
                }
            }
        }

        public bool IsScreenPointOverUI(Vector2 position)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = position;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            return results.Count > 0;
        }

        private void MoveStarted()
        {
            if (_zooming == false && (UI_Main.instance.isActive || UI_Battle.instance.isActive || UI_WarLayout.instance.isActive || UI_Scout.instance.isActive) && UI_Chat.instance.isActive == false && UI_Settings.instance.isActive == false && UI_BuildingUpgrade.instance.isActive == false && UI_Info.instance.isActive == false && UI_Store.instance.isActive == false)
            {
                if (UI_WarLayout.instance.isActive)
                {
                    PointerEventData data = new PointerEventData(EventSystem.current);
                    data.position = inputs.Main.PointerPosition.ReadValue<Vector2>();
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(data, results);
                    if (results.Count > 0)
                    {
                        return;
                    }
                }
                if (_building)
                {
                    _buildBasePosition = CameraScreenPositionToWorldPosition(inputs.Main.PointerPosition.ReadValue<Vector2>());
                    Vector3Int gridPos = UI_Main.instance._grid.grid.WorldToCell(_buildBasePosition);
                    if (UI_Main.instance._grid.IsGridPositionIsOnBuilding(new Vector2Int(gridPos.x, gridPos.y), Building.buildinstance.currentX, Building.buildinstance.currentY, Building.buildinstance.rows, Building.buildinstance.columns))
                    {
                        Building.buildinstance.StartMovingOnGrid();
                        _movingBuilding = true;
                    }
                }

                if(Building.selectedinstance != null && Building.selectedinstance.id != Data.BuildingID.obstacle && !Building.selectedinstance.scout && Building.selectedinstance.databaseID > 0)
                {
                    _replaceBasePosition = CameraScreenPositionToWorldPosition(inputs.Main.PointerPosition.ReadValue<Vector2>());
                    Vector3Int gridPos = UI_Main.instance._grid.grid.WorldToCell(_replaceBasePosition);
                    if (UI_Main.instance._grid.IsGridPositionIsOnBuilding(new Vector2Int(gridPos.x, gridPos.y), Building.selectedinstance.currentX, Building.selectedinstance.currentY, Building.selectedinstance.rows, Building.selectedinstance.columns))
                    {
                        if (!_replacing)
                        {
                            _replacing = true;
                        }
                        Building.selectedinstance.StartMovingOnGrid();
                        _replacingBuilding = true;
                    }
                }

                if(_movingBuilding == false && _replacingBuilding == false)
                {
                    _moveRootBasePosition = _root.position;
                    _moveInputBaseScreenPosition = inputs.Main.PointerPosition.ReadValue<Vector2>();
                    _moveBaseDirection = new Vector2((_camera.orthographicSize * _camera.aspect * 2f) / Screen.width, (_camera.orthographicSize * 2f) / Screen.height);
                    _moving = true;
                }
            }
        }

        private void MoveCanceled()
        {
            _moving = false;
            _movingBuilding = false;
            if (_replacingBuilding)
            {
                _replacingBuilding = false;
                if (Building.selectedinstance)
                {
                    Building.selectedinstance.SaveLocation(false);
                }
            }
        }

        private void ZoomStarted()
        {
            if ((UI_Main.instance.isActive || UI_Battle.instance.isActive || UI_WarLayout.instance.isActive || UI_Scout.instance.isActive) && UI_Chat.instance.isActive == false && UI_Settings.instance.isActive == false && UI_BuildingUpgrade.instance.isActive == false && UI_Info.instance.isActive == false && UI_Store.instance.isActive == false)
            {
                _moveRootBasePosition = _root.position;
                Vector2 touch0 = inputs.Main.TouchPosition0.ReadValue<Vector2>();
                Vector2 touch1 = inputs.Main.TouchPosition1.ReadValue<Vector2>();
                _zoomPositionOnScreen = Vector2.Lerp(touch0, touch1, 0.5f);
                _zoomPositionInWorld = CameraScreenPositionToWorldPosition(_zoomPositionOnScreen);
                _zoomBaseValue = _zoom;

                touch0.x /= Screen.width;
                touch1.x /= Screen.width;
                touch0.y /= Screen.height;
                touch1.y /= Screen.height;

                _zoomBaseDistance = Vector2.Distance(touch0, touch1);
                _zooming = true;
                _moving = false;
            }
        }

        private void ZoomCanceled()
        {
            _zooming = false;
        }
        
        private void Update()
        {
            if (UnityEngine.Input.touchSupported == false)
            {
                float mouseScroll = inputs.Main.MouseScroll.ReadValue<float>();
                if(mouseScroll > 0)
                {
                    _zoom -= _mouseZoomSpeed * Time.deltaTime;
                }
                else if (mouseScroll < 0)
                {
                    _zoom += _mouseZoomSpeed * Time.deltaTime;
                }
            }

            if (_zooming)
            {
                Vector2 touch0 = inputs.Main.TouchPosition0.ReadValue<Vector2>();
                Vector2 touch1 = inputs.Main.TouchPosition1.ReadValue<Vector2>();

                touch0.x /= Screen.width;
                touch1.x /= Screen.width;
                touch0.y /= Screen.height;
                touch1.y /= Screen.height;

                float currentDistance = Vector2.Distance(touch0, touch1);
                float deltaDistance = currentDistance - _zoomBaseDistance;
                _zoom = _zoomBaseValue - (deltaDistance * _zoomSpeed);

                Vector3 zoomCenter = CameraScreenPositionToWorldPosition(_zoomPositionOnScreen);
                _root.position = _moveRootBasePosition + (_zoomPositionInWorld - zoomCenter);
            }
            else if (_moving)
            {
                //Vector2 delta = inputs.Main.PointerPosition.ReadValue<Vector2>() - _moveBasePosition;
                //_root.position = _moveRootPosition - new Vector3(delta.x * _moveBaseDirection.x, 0, delta.y * _moveBaseDirection.y * 2f);

                Vector2 delta = inputs.Main.PointerPosition.ReadValue<Vector2>() - _moveInputBaseScreenPosition;
                _root.position = _moveRootBasePosition - new Vector3(delta.x * _moveBaseDirection.x, delta.y * _moveBaseDirection.y, 0);
            }

            AdjustBounds();

            if (Mathf.Abs(_camera.orthographicSize - _zoom) >= 0.01f)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _zoom, _zoomSmooth * Time.deltaTime);
            }
            else
            {
                _camera.orthographicSize = _zoom;
            }
            if (_camera.transform.position != _target.position)
            {
                Vector3 velocity = Vector3.zero;
                _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, _target.position, ref velocity, _moveSmooth * Time.deltaTime);
            }
            if (_camera.transform.rotation != _target.rotation)
            {
                _camera.transform.rotation = _target.rotation;
            }

            if (_building && _movingBuilding)
            {
                Vector3 pos = CameraScreenPositionToWorldPosition(inputs.Main.PointerPosition.ReadValue<Vector2>());
                Building.buildinstance.UpdateGridPosition(_buildBasePosition, pos);
            }

            if (_replacing && _replacingBuilding)
            {
                Vector3 pos = CameraScreenPositionToWorldPosition(inputs.Main.PointerPosition.ReadValue<Vector2>());
                Building.selectedinstance.UpdateGridPosition(_replaceBasePosition, pos);
            }

            planDownLeft = CameraScreenPositionToWorldPosition(Vector2.zero);
            planTopRight = CameraScreenPositionToWorldPosition(new Vector2(Screen.width, Screen.height));
        }

        private void AdjustBounds()
        {
            if (_zoom < _zoomMin)
            {
                _zoom = _zoomMin;
            }
            if (_zoom > _zoomMax)
            {
                _zoom = _zoomMax;
            }

            float h = (UI_Main.instance._grid.up + UI_Main.instance._grid.down);
            float w = (UI_Main.instance._grid.right + UI_Main.instance._grid.left);

            float ch = _zoom * 2f;
            float cw = ch * _camera.aspect;

            if (ch > h)
            {
                _zoom = h / 2f;
            }
            if (cw > w)
            {
                _zoom = (w / _camera.aspect) / 2f;
            }

            ch = _zoom;
            cw = ch * _camera.aspect;

            Vector3 position = _root.position;
            if (position.x > UI_Main.instance._grid.grid.transform.position.x + UI_Main.instance._grid.right - cw)
            {
                position.x = UI_Main.instance._grid.grid.transform.position.x + UI_Main.instance._grid.right - cw;
            }
            if (position.x < UI_Main.instance._grid.grid.transform.position.x - UI_Main.instance._grid.left + cw)
            {
                position.x = UI_Main.instance._grid.grid.transform.position.x - UI_Main.instance._grid.left + cw;
            }
            if (position.y > UI_Main.instance._grid.grid.transform.position.y + UI_Main.instance._grid.up - ch)
            {
                position.y = UI_Main.instance._grid.grid.transform.position.y + UI_Main.instance._grid.up - ch;
            }
            if (position.y < UI_Main.instance._grid.grid.transform.position.y - UI_Main.instance._grid.down + ch)
            {
                position.y = UI_Main.instance._grid.grid.transform.position.y - UI_Main.instance._grid.down + ch;
            }
            _root.position = position;
        }

        private Vector3 CameraScreenPositionToWorldPosition(Vector2 position)
        {
            float h = _camera.orthographicSize * 2f;
            float w = _camera.aspect * h;
            Vector3 ancher = _camera.transform.position - (_camera.transform.right.normalized * w / 2f) - (_camera.transform.up.normalized * h / 2f);
            Vector3 world = ancher + (_camera.transform.right.normalized * position.x / Screen.width * w) + (_camera.transform.up.normalized * position.y / Screen.height * h);
            world.z = 0;
            return world;
        }
        /*
        public Vector3 CameraScreenPositionToPlanePosition(Vector2 position)
        {
            Vector3 point = CameraScreenPositionToWorldPosition(position);
            float h = point.y - _root.position.y;
            float x = h / Mathf.Sin(0 * Mathf.Deg2Rad);
            return point + _camera.transform.forward.normalized * x;
        }
        */
    }
}