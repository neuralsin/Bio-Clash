namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;

    public class UI_Main : MonoBehaviour
    {
        
        //[SerializeField] public TextMeshProUGUI logTest = null;
        [SerializeField] public GameObject _elements = null;
        [SerializeField] public TextMeshProUGUI _goldText = null;
        [SerializeField] public TextMeshProUGUI _elixirText = null;
        [SerializeField] public TextMeshProUGUI _darkText = null;
        [SerializeField] public TextMeshProUGUI _gemsText = null;
        [SerializeField] public TextMeshProUGUI _usernameText = null;
        [SerializeField] public TextMeshProUGUI _xpText = null;
        [SerializeField] public TextMeshProUGUI _trophiesText = null;
        [SerializeField] public TextMeshProUGUI _levelText = null;
        [SerializeField] public Image _goldBar = null;
        [SerializeField] public Image _elixirBar = null;
        [SerializeField] public Image _darkBar = null;
        [SerializeField] public Image _gemsBar = null;
        [SerializeField] public Image _xpBar = null;
        [SerializeField] private Button _shopButton = null;
        [SerializeField] private Button _battleButton = null;
        [SerializeField] private Button _chatButton = null;
        [SerializeField] private Button _settingsButton = null;
        [SerializeField] private Button _rankingButton = null;
        [SerializeField] public TextMeshProUGUI _buildersText = null;
        [SerializeField] public TextMeshProUGUI _shieldText = null;
        [SerializeField] private Button _addGemsButton = null;
        [SerializeField] private Button _addShieldButton = null;
        [SerializeField] private Button _buyResourceButton = null;
        [SerializeField] private Button _battleReportsButton = null;
        [SerializeField] private Button _fitnessButton = null; // Bio-Clash Fitness
        [SerializeField] private GameObject _battleReportsNew = null;
        [SerializeField] public BuildGrid _grid = null;
        [SerializeField] public Building[] _buildingPrefabs = null;
        [SerializeField] public List<BattleUnit> _armyCampsUnit = new List<BattleUnit>();

        [Header("Buttons")]
        public Transform buttonsParent = null;
        public UI_Button buttonCollectGold = null;
        public UI_Button buttonCollectElixir = null;
        public UI_Button buttonCollectDarkElixir = null;
        public UI_Bar barBuild = null;
        private static UI_Main _instance = null; public static UI_Main instance { get { return _instance; } }

        private bool _active = true;public bool isActive { get { return _active; } }
        private int workers = 0;
        private int busyWorkers = 0; public bool haveAvailableBuilder { get { return busyWorkers < workers; } }

        /*
        public void Log(string text)
        {
            logTest.text = logTest.text + "\n" + text;
        }
        */

        private void Awake()
        {
             _instance = this;
            _elements.SetActive(true);
            
            // BIO-CLASH: Hide standard resources
            if (_goldBar) _goldBar.transform.parent.gameObject.SetActive(false);
            if (_elixirBar) _elixirBar.transform.parent.gameObject.SetActive(false);
            if (_darkBar) _darkBar.transform.parent.gameObject.SetActive(false);
            if (_gemsBar) _gemsBar.transform.parent.gameObject.SetActive(false); // Gems might still be useful, keeping hidden for now per user request for "total replacement" logic
            
            _usernameText.text = "";
            _xpText.text = "";
            _trophiesText.text = "";
            _levelText.text = "";
            _buildersText.text = "";
            _shieldText.text = "";
        }

        private void Start()
        {
            _shopButton.onClick.AddListener(ShopButtonClicked);
            _battleButton.onClick.AddListener(BattleButtonClicked);
            _chatButton.onClick.AddListener(ChatButtonClicked);
            _settingsButton.onClick.AddListener(SettingsButtonClicked);
            _addGemsButton.onClick.AddListener(AddGems);
            _addShieldButton.onClick.AddListener(AddShield);
            _rankingButton.onClick.AddListener(RankingButtonClicked);
            _buyResourceButton.onClick.AddListener(BuyResource);
            _battleReportsButton.onClick.AddListener(BattleReportsButtonClicked);
            if (_fitnessButton != null)
            {
                _fitnessButton.onClick.AddListener(FitnessButtonClicked);
                // Pulse effect to highlight it
                StartCoroutine(PulseFitnessButton());
            }
            SoundManager.instance.PlayMusic(SoundManager.instance.mainMusic);
        }

        private IEnumerator PulseFitnessButton()
        {
            // BIO-CLASH: Added safety checks to prevent null ref on scene change
            while (this != null && gameObject != null && gameObject.activeInHierarchy)
            {
                if (_fitnessButton != null && _fitnessButton.gameObject != null)
                {
                    _fitnessButton.transform.localScale = Vector3.one * (1f + Mathf.PingPong(Time.time * 0.5f, 0.1f));
                }
                else
                {
                    yield break; // Exit if button is destroyed
                }
                yield return null;
            }
        }

        public void ChangeUnreadBattleReports(int count)
        {
            _battleReportsNew.SetActive(count > 0);
        }

        private void BattleReportsButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_BattleReports.instance.Open();
        }

        private void FitnessButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Fitness.instance.Open();
        }

        private void SettingsButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Settings.instance.Open();
        }

        private void RankingButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_PlayersRanking.instance.Open();
        }

        private void ChatButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Chat.instance.Open();
        }

        private void ShopButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Shop.instance.SetStatus(true);
            SetStatus(false);
        }

        private void BattleButtonClicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Search.instance.SetStatus(true);
            SetStatus(false);
        }

        private void OnLeave()
        {
            UI_Build.instance.Cancel();
        }

        public void SetStatus(bool status)
        {
            if (!status)
            {
                OnLeave();
            }
            else
            {
                if (SoundManager.instance.musicSource.clip != SoundManager.instance.mainMusic)
                {
                    SoundManager.instance.PlayMusic(SoundManager.instance.mainMusic);
                }
                Player.instance.RushSyncRequest();
            }
            _active = status;
            _elements.SetActive(status);
        }

        public (Building, Data.ServerBuilding) GetBuildingPrefab(Data.BuildingID id)
        {
            Data.ServerBuilding server = Player.instance.GetServerBuilding(id, 1);
            if (server != null)
            {
                for (int i = 0; i < _buildingPrefabs.Length; i++)
                {
                    if (_buildingPrefabs[i].id == id)
                    {
                        return (_buildingPrefabs[i], server);
                    }
                }
            }
            return (null, null);
        }

        public void DataSynced()
        {
            int _workers = 0;
            int _busyWorkers = 0;
            if (Player.instance.data.buildings != null && Player.instance.data.buildings.Count > 0)
            {
                for (int i = 0; i < Player.instance.data.buildings.Count; i++)
                {
                    bool first = false;
                    if (Player.instance.data.buildings[i].isConstructing && Player.instance.data.buildings[i].buildTime > 0)
                    {
                        _busyWorkers += 1;
                    }
                    Building building = _grid.GetBuilding(Player.instance.data.buildings[i].databaseID);
                    if (building != null)
                    {
                        
                    }
                    else
                    {
                        building = _grid.GetBuilding(Player.instance.data.buildings[i].id, Player.instance.data.buildings[i].x, Player.instance.data.buildings[i].y);
                        if(building != null)
                        {
                            _grid.RemoveUnidentifiedBuilding(building);
                            building.databaseID = Player.instance.data.buildings[i].databaseID;
                            _grid.buildings.Add(building);
                        }
                        else
                        {
                            var prefab = GetBuildingPrefab(Player.instance.data.buildings[i].id);
                            if (prefab.Item1)
                            {
                                building = Instantiate(prefab.Item1, Vector3.zero, Quaternion.identity);
                                building.rows = prefab.Item2.rows;
                                building.columns = prefab.Item2.columns;
                                building.databaseID = Player.instance.data.buildings[i].databaseID;
                                building.lastChange = Player.instance.lastUpdateSent.AddSeconds(-1);
                                first = true;
                                building.PlacedOnGrid(Player.instance.data.buildings[i].x, Player.instance.data.buildings[i].y);
                                if (building._baseArea)
                                {
                                    building._baseArea.gameObject.SetActive(false);
                                }
                                _grid.buildings.Add(building);
                            }
                            else
                            {
                                Debug.LogWarning("Building " + Player.instance.data.buildings[i].id + " have no prefab.");
                                continue;
                            }
                        }
                    }

                    if (building.buildBar == null)
                    {
                        building.buildBar = Instantiate(barBuild, buttonsParent);
                        building.buildBar.gameObject.SetActive(false);
                    }

                    building.data = Player.instance.data.buildings[i];
                    if(first)
                    {
                        building.lastChange = Player.instance.lastUpdateSent.AddSeconds(-1);
                    }

                    switch (building.id)
                    {
                        case Data.BuildingID.goldmine:
                            if (building.collectButton == null)
                            {
                                building.collectButton = Instantiate(buttonCollectGold, buttonsParent);
                                building.collectButton.button.onClick.AddListener(building.Collect);
                                building.collectButton.gameObject.SetActive(false);
                            }
                            break;
                        case Data.BuildingID.elixirmine:
                            if (building.collectButton == null)
                            {
                                building.collectButton = Instantiate(buttonCollectElixir, buttonsParent);
                                building.collectButton.button.onClick.AddListener(building.Collect);
                                building.collectButton.gameObject.SetActive(false);
                            }
                            break;
                        case Data.BuildingID.darkelixirmine:
                            if (building.collectButton == null)
                            {
                                building.collectButton = Instantiate(buttonCollectDarkElixir, buttonsParent);
                                building.collectButton.button.onClick.AddListener(building.Collect);
                                building.collectButton.gameObject.SetActive(false);
                            }
                            break;
                        case Data.BuildingID.buildershut:
                            _workers += 1;
                            break;
                    }
                }
                _grid.RefreshBuildings();
            }
            if (Player.instance.data.buildings != null)
            {
                for (int i = _grid.buildings.Count - 1; i >= 0; i--)
                {
                    bool found = false;
                    for (int j = 0; j < Player.instance.data.buildings.Count; j++)
                    {
                        if (_grid.buildings[i].data.databaseID == Player.instance.data.buildings[j].databaseID)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // BIO-CLASH: Delayed destruction to avoid visual popping on network lag
                        // Buildings fade out smoothly instead of instant destruction
                        Building buildingToDestroy = _grid.buildings[i];
                        _grid.buildings.RemoveAt(i);
                        StartCoroutine(DelayedDestroyBuilding(buildingToDestroy, 0.5f));
                    }
                }
            }
            workers = _workers;
            busyWorkers = _busyWorkers;
            _buildersText.text = (_workers - _busyWorkers).ToString() + "/" + _workers.ToString();
        }

        private void Update()
        {
            if (_active)
            {
                if(Player.instance.data.shield > Player.instance.data.nowTime)
                {
                    _shieldText.text = Tools.SecondsToTimeFormat((int)(Player.instance.data.shield - Player.instance.data.nowTime).TotalSeconds);
                }
                else
                {
                    _shieldText.text = "None";
                }
            }
        }

        private void AddShield()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Store.instance.Open(2);
        }

        private void AddGems()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Store.instance.Open(1);
        }

        private void BuyResource()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Store.instance.Open(3);
        }

        /// <summary>
        /// BIO-CLASH: Delayed building destruction with fade-out effect.
        /// Prevents visual popping when buildings are removed on network sync.
        /// </summary>
        private IEnumerator DelayedDestroyBuilding(Building building, float delay)
        {
            if (building == null || building.gameObject == null) yield break;
            
            // Optional: Add fade-out visual effect here
            // For now, just wait then destroy
            float elapsed = 0f;
            Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
            
            while (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / delay);
                
                // Fade out all renderers
                foreach (Renderer r in renderers)
                {
                    if (r != null && r.material != null)
                    {
                        Color c = r.material.color;
                        c.a = alpha;
                        r.material.color = c;
                    }
                }
                yield return null;
            }
            
            if (building != null && building.gameObject != null)
            {
                Destroy(building.gameObject);
            }
        }

    }
}