using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController
{
    private static GameController _instance;
    
    // Этот метод выполнится САМ при запуске игры
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Debug.Log("GameController: Глобальная инициализация...");
        // Тут можно создать глобальный объект, который не удаляется
        _instance = new GameController();
        
        _instance._gameSettingsAsset = Resources.Load<GameSettingsAsset>("GameSettingsAsset");

        Application.quitting += SavingToFile;
        Application.quitting += UnloadAsset;

        if (SaveManager.DataDeserialized || SaveManager.Deserialize())
        {
            _instance._coinsCount = SaveManager.Data.coinsCount;

            _instance.GameSettingsAsset.ShipSpeed.CurrentLevelIndex = SaveManager.Data.shipSpeedLevel;
            _instance.GameSettingsAsset.FireRate.CurrentLevelIndex = SaveManager.Data.fireRateLevel; 
            _instance.GameSettingsAsset.BulletSpeed.CurrentLevelIndex = SaveManager.Data.bulletSpeedLevel;
            _instance.GameSettingsAsset.BulletDamage.CurrentLevelIndex = SaveManager.Data.bulletDamageLevel;
        }
    }

    public static GameController Instance
    {
        get
        {
            if(_instance == null) throw new Exception("GameController instance is null");
            return _instance; 
        }
    }
    
    private GameSettingsAsset _gameSettingsAsset;
    
    private int _health;
    private int _healthLimit = 200;
    private string _startScene = "Start";
    private int _coinsCount;
    public int _shipSpeedLevel;
    public int _fireRateLevel;
    public int _bulletSpeedLevel;
    public int _bulletDamageLevel;
    
    public GameSettingsAsset GameSettingsAsset => _gameSettingsAsset;
    public int Health => _health;
    public int HealthLimit => _healthLimit;
    public int CoinsCount => _coinsCount;
    public int ShipSpeedLevel => _shipSpeedLevel;
    public int FireRateLevel => _fireRateLevel;
    public int BulletSpeedLevel => _bulletSpeedLevel;
    public int BulletDamageLevel => _bulletDamageLevel;
   
    public event Action<int> OnTakeDamageShip;
    public event Action<int> OnAddCoins;
    public event Action<int> OnRemoveCoins;
    
    public event Action OnDeathShip;
    public event Action<int> OnHealthChange;
    
    public void SetStartParameters()
    {
        _health = _healthLimit;
    }
    
    //TODO на кнопку выхода в главное меню
    public void ResetStartParameters()
    {
        _health = 0;
        //Debug.Log($"health = {_health}");
    }
    public void TakeDamage(int damage)
    {
        if(damage == 0) return;
        if (damage < 0) throw new ArgumentException("Отрицательный урон", nameof(damage));
   
      if (_health <= 0) return;
        
        _health -= damage;
        
        OnTakeDamageShip?.Invoke(damage);
        
        if (_health < 0)
            _health = 0;
        
        OnHealthChange?.Invoke(_health);
        
        if (_health == 0)
            DieShip();
    }

    public void AddCoins(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if(count == 0) return;
        
        _coinsCount += count;
        OnAddCoins?.Invoke(_coinsCount);
    }

    public void RemoveCoins(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if(count == 0) return;
        
        _coinsCount -= count;
        OnRemoveCoins?.Invoke(_coinsCount);
    }
    void DieShip()
    {
        Debug.Log("Корабль разрушен! Перезагрузить игру");
        OnDeathShip?.Invoke();
    }

    public void LoadStartScene()
    {
        SceneManager.LoadScene(_startScene);
    }
    
    private static void SavingToFile()
    {
        if (SaveManager.Data == null)
            SaveManager.CreateSavedDataInstance();
        
        SaveManager.Data.coinsCount = _instance._coinsCount;
        SaveManager.Data.shipSpeedLevel = _instance.GameSettingsAsset.ShipSpeed.CurrentLevelIndex;
        SaveManager.Data.fireRateLevel = _instance.GameSettingsAsset.FireRate.CurrentLevelIndex;
        SaveManager.Data.bulletSpeedLevel = _instance.GameSettingsAsset.BulletSpeed.CurrentLevelIndex;
        SaveManager.Data.bulletDamageLevel = _instance.GameSettingsAsset.BulletDamage.CurrentLevelIndex;

        SaveManager.Serialize();
    }

    private static void UnloadAsset()
    {
        Resources.UnloadAsset(_instance._gameSettingsAsset);
        _instance._gameSettingsAsset = null;
    }
}
