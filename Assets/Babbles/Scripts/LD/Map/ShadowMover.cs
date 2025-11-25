using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using ClemCAddons.Utilities;

namespace ClemCAddons
{
    namespace Player
    {
        [ExecuteInEditMode]
        public class ShadowMover : MonoBehaviour
        {
            [SerializeField] private float _scanDistance = 50;
            [SerializeField] private float _fadeDistance = 30;
            private CharacterMovement _player;
            private SpriteRenderer _sprite;

            public SpriteRenderer Sprite
            {
                get
                {
                    if (_sprite == null)
                    {
                        _sprite = GetComponent<SpriteRenderer>();
                    }
                    return _sprite;
                }
                set
                {
                    _sprite = value;
                }
            }

            // Start is called before the first frame update
            void Start()
            {
                _player = FindObjectOfType<CharacterMovement>();
                Sprite = GetComponent<SpriteRenderer>();
            }

            // Update is called once per frame
            void Update()
            {
                float bounds = _player.GetComponent<Collider>().bounds.extents.y;
                float distance = GameTools.FindGround(_player.transform.position, bounds, _scanDistance, _player.CollisionLayer);
                // physics are moving between the previous lastupdate and this update, so can't use the player's
                if (distance < _scanDistance)
                {
                    Vector3 pos = _player.transform.position;
                    pos.y -= distance + bounds - 0.01f;
                    transform.position = pos;
                    Sprite.color = new Color(Sprite.color.r, Sprite.color.g, Sprite.color.b, 1 - (Mathf.Clamp(distance,0,_fadeDistance) / _fadeDistance));
                }
                else
                {
                    Sprite.color = new Color(Sprite.color.r, Sprite.color.g, Sprite.color.b, 0);
                }
            }
        }
    }
}