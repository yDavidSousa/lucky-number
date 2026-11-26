using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class InteractionSystem
{
    public struct PointerInteractionData
    {
        public int id;
        public bool enabled;

        public bool hovered;
        public bool pressed;
        public bool clicked;
    }

    private struct Entry
    {
        public Collider Collider;
        public PointerInteractionData Data;
    }

    private readonly Camera _camera;

    private readonly List<Entry> _entries = new();
    private readonly Dictionary<int, int> _indexById = new();
    private readonly Dictionary<Collider, int> _idByCollider = new();
    private int? _pressedId;

    public InteractionSystem(Camera camera)
    {
        _camera = camera;
    }

    public void Register(int id, Collider collider)
    {
        if (collider == null)
        {
            throw new ArgumentNullException(nameof(collider));
        }

        if (_indexById.ContainsKey(id))
        {
            throw new InvalidOperationException($"Selectable ID {id} já está registrado.");
        }

        if (_idByCollider.ContainsKey(collider))
        {
            throw new InvalidOperationException($"Collider {collider.name} já está registrado.");
        }

        _indexById.Add(id, _entries.Count);
        _idByCollider.Add(collider, id);

        _entries.Add(new Entry
        {
            Collider = collider,
            Data = new PointerInteractionData
            {
                id = id,
                enabled = true
            }
        });
    }

    public void Unregister(int id)
    {
        if (!_indexById.TryGetValue(id, out int index))
            return;

        Entry removed = _entries[index];

        _idByCollider.Remove(removed.Collider);
        _indexById.Remove(id);

        // Preenche o espaço removido com o último registro.
        int lastIndex = _entries.Count - 1;

        if (index != lastIndex)
        {
            Entry moved = _entries[lastIndex];

            _entries[index] = moved;
            _indexById[moved.Data.id] = index;
        }

        _entries.RemoveAt(lastIndex);

        if (_pressedId == id)
            _pressedId = null;
    }
    
    public void Clear()
    {
        _entries.Clear();
        _indexById.Clear();
        _idByCollider.Clear();

        _pressedId = null;
    }
    
    private int? Raycast(Camera camera, Vector2 pointerPosition, LayerMask raycastMask, float maxDistance)
    {
        Ray ray = camera.ScreenPointToRay(pointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        if (!_idByCollider.TryGetValue(hit.collider, out int id))
            return null;

        return CanInteract(id) ? id : null;
    }

    private bool CanInteract(int id)
    {
        if (!_indexById.TryGetValue(id, out int index))
            return false;

        Entry entry = _entries[index];
        return entry.Data.enabled;
    }

    public void Tick(LayerMask raycastMask, float maxDistance = 100f)
    {
        Vector2 pointerPosition = Mouse.current.position.ReadValue();
        bool pointerDown = Mouse.current.leftButton.wasPressedThisFrame;
        bool pointerUp = Mouse.current.leftButton.wasReleasedThisFrame;
        bool pointerBlocked = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        int? hoveredId = null;
        int? clickedId = null;

        if (_pressedId.HasValue && !CanInteract(_pressedId.Value))
        {
            _pressedId = null;
        }

        if (pointerBlocked)
        {
            _pressedId = null;
        }
        else
        {
            hoveredId = Raycast(_camera, pointerPosition, raycastMask, maxDistance);

            if (pointerDown)
            {
                _pressedId = hoveredId;   
            }

            if (pointerUp)
            {
                if (_pressedId.HasValue && _pressedId == hoveredId)
                {
                    clickedId = _pressedId;
                }

                _pressedId = null;
            }
        }

        for (int i = 0; i < _entries.Count; i++)
        {
            Entry entry = _entries[i];
            int id = entry.Data.id;

            entry.Data.hovered = hoveredId == id;
            entry.Data.pressed = _pressedId == id;
            entry.Data.clicked = clickedId == id;

            _entries[i] = entry;
        }
    }

    public bool TryGetData(int id, out PointerInteractionData data)
    {
        if (_indexById.TryGetValue(id, out int index))
        {
            data = _entries[index].Data;
            return true;
        }

        data = default;
        return false;
    }

    public void SetEnabled(int id, bool enabled)
    {
        if (!_indexById.TryGetValue(id, out int index))
            return;

        Entry entry = _entries[index];
        entry.Data.enabled = enabled;

        if (!enabled)
        {
            entry.Data.hovered = false;
            entry.Data.pressed = false;
            entry.Data.clicked = false;

            if (_pressedId == id)
                _pressedId = null;
        }

        _entries[index] = entry;
    }
}