using System;
using BepInEx.Configuration;

namespace CSync.Lib;

/// <summary>
/// Wrapper class around a BepInEx <see cref="ConfigEntry{T}"/>.<br>
/// Can serialize and deserialize itself to avoid runtime errors when syncing configs.</br>
/// </summary>
public sealed class SyncedEntry<T> : SyncedEntryBase
{
    private ConfigEntry<T> _entry;

    public ConfigEntry<T> Entry {
        get => _entry;
        init {
            _entry = value;
            _lastLocalValue = _entry.Value;
            Entry.SettingChanged += OnLocalSettingChanged;
        }
    }

    public override ConfigEntryBase BoxedEntry {
        get => Entry;
        protected init => Entry = (ConfigEntry<T>)value;
    }

    public override bool ValueOverridden {
        get => base.ValueOverridden;
        internal set
        {
            if (value == base.ValueOverridden) return;

            var lastValue = Value;
            base.ValueOverridden = value;
            OnValueOverriddenChanged(this, lastValue);
        }
    }

    private void OnLocalSettingChanged(object sender, EventArgs e)
    {
        InvokeChangedIfNecessary();
        _lastLocalValue = LocalValue;
        return;

        void InvokeChangedIfNecessary()
        {
            if (ValueOverridden) return;
            if (Equals(_lastLocalValue, LocalValue)) return;

            var args = new SyncedSettingChangedEventArgs<T>
            {
                OldValue = _lastLocalValue,
                NewValue = LocalValue,
                ChangedEntry = this,
            };
            Changed?.Invoke(sender, args);
        }
    }

    private void OnValueOverrideChanged(object sender, T oldValue)
    {
        if (!ValueOverridden) return;
        if (Equals(oldValue, _typedValueOverride)) return;

        var args = new SyncedSettingChangedEventArgs<T>
        {
            OldValue = oldValue,
            NewValue = _typedValueOverride,
            ChangedEntry = this,
        };
        Changed?.Invoke(sender, args);
    }

    private void OnValueOverriddenChanged(object sender, T oldValue)
    {
        if (Equals(Value, oldValue)) return;

        var args = new SyncedSettingChangedEventArgs<T>
        {
            OldValue = oldValue,
            NewValue = Value,
            ChangedEntry = this,
        };
        Changed?.Invoke(sender, args);
    }

    public event EventHandler<SyncedSettingChangedEventArgs<T>>? Changed;

    private T _lastLocalValue;

    public T LocalValue
    {
        get => Entry.Value;
        set => Entry.Value = value!;
    }

    private T _typedValueOverride;

    public override object? BoxedValueOverride
    {
        get => _typedValueOverride;
        set
        {
            var lastValue = Value;
            _typedValueOverride = (T)value!;
            OnValueOverrideChanged(this, lastValue);
        }
    }

    public T Value {
        get {
            if (ValueOverridden) return _typedValueOverride!;
            return LocalValue;
        }
    }

    public static implicit operator T(SyncedEntry<T> e) => e.Value;

    public SyncedEntry(ConfigEntry<T> entry) : base(entry) { }

    public override string ToString() {
        return $"Key: {Entry.Definition.Key}\nLocal Value: {LocalValue}\nCurrent Value: {Value}";
    }
}
