using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class LetterTile : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] protected TextMeshPro _letterText;
    [SerializeField] protected SpriteRenderer _bgRenderer;

    protected bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            ToggleVisibility(!_isSelected);
        }
    }

    protected Tile _tile;

    private void Start()
    {
        IsSelected = false;
    }

    /// <summary>
    /// Changes the visual appeal of this object to appear like a Tile.
    /// </summary>
    public void InitializeTile(Tile tile)
    {
        _tile = tile;
        _letterText.text = tile.Letters;
    }

    /// <summary>
    /// Toggle whether this tile looks like it's been clicked or not.
    /// 
    /// isVisible = true -> Tile looks like it is interactable
    /// isVisible = false -> Tile looks like it isn't interactable
    /// </summary>
    private void ToggleVisibility(bool isVisible)
    {
        Color bgColor = _bgRenderer.color;
        Color letterColor = _letterText.color;
        _bgRenderer.color = new Color(bgColor.r, bgColor.g, bgColor.b, isVisible ? 1 : 0.3f);
        _letterText.color = new Color(letterColor.r, letterColor.g, letterColor.b, isVisible ? 1 : 0.3f);
    }

}