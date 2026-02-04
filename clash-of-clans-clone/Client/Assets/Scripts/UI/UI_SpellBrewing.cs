namespace DevelopersHub.ClashOfWhatecer
{
    using DevelopersHub.RealtimeNetworking.Client;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_SpellBrewing : MonoBehaviour
    {

        [SerializeField] private Image _bar = null;
        [SerializeField] private Button _buttonRemove = null;
        [SerializeField] private Image _icon = null;

        private Data.Spell _spell = null; public long databaseID { get { return _spell != null ? _spell.databaseID : 0; } }
        [HideInInspector] public Data.SpellID id = Data.SpellID.healing;

        [HideInInspector] public int index = -1;
        private bool _remove = false; public bool remove { get { return _remove; } }

        private void Start()
        {
            _buttonRemove.onClick.AddListener(Remove);
        }

        public void Initialize(Data.Spell spell)
        {
            _bar.fillAmount = 0;
            _spell = spell;
            Sprite icon = AssetsBank.GetSpellIcon(spell.id);
            if (icon != null)
            {
                _icon.sprite = icon;
            }
            if (_remove)
            {
                Remove();
            }
        }

        public bool Initialize(Data.SpellID id)
        {
            bool haveResources = false;
            this.id = id;
            _bar.fillAmount = 0;
            _spell = null;
            int level = 1;
            for (int i = 0; i < Player.instance.initializationData.research.Count; i++)
            {
                if (Player.instance.initializationData.research[i].type == Data.ResearchType.spell && Player.instance.initializationData.research[i].globalID == id.ToString())
                {
                    level = Player.instance.initializationData.research[i].level;
                    break;
                }
            }
            for (int i = 0; i < Player.instance.initializationData.serverSpells.Count; i++)
            {
                if (Player.instance.initializationData.serverSpells[i].id == id && Player.instance.initializationData.serverSpells[i].level == level)
                {
                    _spell = new Data.Spell();
                    _spell.id = id;
                    _spell.brewed = false;
                    _spell.ready = false;
                    _spell.level = level;
                    _spell.databaseID = 0;
                    _spell.brewedTime = 0;
                    _spell.brewTime = Player.instance.initializationData.serverSpells[i].brewTime;
                    if (Player.instance.data.gems >= Player.instance.initializationData.serverSpells[i].requiredGems && Player.instance.elixir >= Player.instance.initializationData.serverSpells[i].requiredElixir && Player.instance.gold >= Player.instance.initializationData.serverSpells[i].requiredGold && Player.instance.darkElixir >= Player.instance.initializationData.serverSpells[i].requiredDarkElixir)
                    {
                        haveResources = true;
                    }
                }
            }
            Sprite icon = AssetsBank.GetSpellIcon(id);
            if (icon != null)
            {
                _icon.sprite = icon;
            }
            return haveResources;
        }

        public void Remove()
        {
            if(_remove)
            {
                return;
            }
            if (databaseID <= 0)
            {
                //_remove = true;
                //transform.GetChild(0).gameObject.SetActive(false);
                //transform.SetParent(null);
                return;
            }
            _remove = true;
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            Packet paket = new Packet();
            paket.Write((int)Player.RequestsID.CANCELBREW);
            paket.Write(_spell.databaseID);
            Sender.TCP_Send(paket);
            UI_Spell.instance.RemoveTrainingItem(index);
        }

        private void Update()
        {
            if (_spell != null)
            {
                if (index == 0)
                {
                    _spell.brewedTime += Time.deltaTime;
                    if (_spell.brewTime > 0)
                    {
                        float fill = _spell.brewedTime / _spell.brewTime;
                        if (fill > 1f)
                        {
                            fill = 1f;
                        }
                        _bar.fillAmount = fill;
                    }
                }
                if (_spell.brewTime <= 0 || _spell.brewedTime >= _spell.brewTime)
                {
                    _bar.fillAmount = 1f;
                    for (int i = Player.instance.data.spells.Count - 1; i >= 0; i--)
                    {
                        if (Player.instance.data.spells[i].databaseID == databaseID)
                        {
                            Player.instance.data.spells[i].ready = true;
                            break;
                        }
                    }
                    UI_Spell.instance.RemoveTrainingItem(index);
                }
            }
        }

    }
}