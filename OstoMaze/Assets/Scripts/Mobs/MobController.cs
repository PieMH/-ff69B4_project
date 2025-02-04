﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobController : MonoBehaviour, IEnemy {
    public float TIMER;
    public float VELOCITY; // Velocita di movimento
    public float DIST_MOVEMENT;
    public string char_name;
    public float _timer;
    public float hp;
    public bool death = false;

    public Animator mob_anim;

    private Vector3 _offset;
    private Vector3[] _directions = new Vector3[4];
    private Vector3 _curr_direction = Vector3.zero;

    private Rigidbody2D _rb;
    private GameObject _player;
    private AudioSource[] sounds;
    private int index;  // index for AudioSource array
    private SpriteRenderer _sprite;
    private Projectile _projectile;
    public Food[] foods;

    public enum State {SPAWNING, IDLE, MOVING, ATTACKING};
    public State _curr_state;

    public GameObject getPlayer() {
        return _player;
    }

    public void ChangeState(State new_state) {
        _curr_state = new_state;
    }

    private Vector3 first_available_direction() {
        foreach(Vector3 dir in _directions)
            if (!Physics2D.Raycast(transform.position + _offset, dir, DIST_MOVEMENT))
                return dir;
        return Vector3.zero;
    }

    private void shuffle(Vector3[] array) {
        for(int i = 0; i < array.Length; i++) {
            int ridx = Random.Range(0, array.Length-1);
            Vector3 buff = array[i];
            array[i] = array[ridx];
            array[ridx] = buff;
        }
    }

    public void enterRange(GameObject player) {
        mob_anim.SetBool("attack", true);
        mob_anim.SetBool("walk", false);
        _player = player;
        _rb.velocity = Vector3.zero;
        _curr_state = State.ATTACKING;
    }

    public void leaveRange() {
        mob_anim.SetBool("attack", false);
        mob_anim.SetBool("walk", false);
        _curr_state = State.IDLE;
    }

    private void Attack()
    {
        sounds[Random.Range(0, 2)].Play();
        Vector3 spawn = this.transform.position - new Vector3(0, 0.5f, 0);  // mob position - offset
        Projectile newprojectile = Instantiate(this._projectile, spawn, new Quaternion(0,0,0,0));
        Vector3 direction = (_player.transform.position - transform.position).normalized;
        newprojectile.Shoot(spawn, direction);
    }

    public void TakeDamage(float damage) {
        this.hp -= damage; //Damage computation
        mob_anim.SetTrigger("wound");
        if (hp <= 0) {
            mob_anim.Play("Death");
            mob_anim.SetLayerWeight(mob_anim.GetLayerIndex("Wounded"), 0);
        }
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    private void Die() {
        index = Random.Range(2, 4);
        sounds[index].Play();
        if(foods != null  && foods.Length  > 0) {
            int rint = Random.Range(0, foods.Length);
            Food newfood = Instantiate(this.foods[rint], this.transform.position, new Quaternion(0,0,0,0));
            newfood.transform.parent = null;
            newfood.Drop();
        }
        death = true;
    }

    void OnTriggerEnter2D(Collider2D coll) {
        if(coll.gameObject.tag == "Arrow")
            TakeDamage(1);
    }

    private void Move()
    {
        _rb.velocity = _curr_direction * VELOCITY;
        if (Vector3.Dot(_curr_direction, transform.right) < 0)
            _sprite.flipX = true;
        if (Vector3.Dot(_curr_direction, transform.right) > 0)
            _sprite.flipX = false;
    }

    // Start is called before the first frame update
    void Start() {
        _rb = GetComponent<Rigidbody2D>();

        _directions[0] = new Vector3(0, -1, 0);
        _directions[1] = new Vector3(0, 1, 0);
        _directions[2] = new Vector3(-1, 0, 0);
        _directions[3] = new Vector3(1, 0, 0);
        _offset = GetComponent<BoxCollider2D>().offset * transform.localScale.y;
        _sprite = GetComponent<SpriteRenderer>();
        sounds = GetComponents<AudioSource>();
        _projectile = Resources.Load<Projectile>("Projectile");
        _curr_state = State.IDLE;
    }

    // Update is called once per frame
    void Update() {
        switch(_curr_state) {
            case State.SPAWNING:
                break;
            case State.IDLE:
                _timer -= Time.deltaTime;
                if(_timer < 0) {
                    shuffle(_directions);
                    _curr_direction = first_available_direction();
                    if(_curr_direction == Vector3.zero) {
                        _timer = TIMER;
                        break;
                    }
                    _curr_state = State.MOVING;
                    _timer = DIST_MOVEMENT / VELOCITY;
                    mob_anim.SetBool("walk", true);
                    mob_anim.SetBool("attack", false);
                }
                break;
            case State.MOVING:
                _timer -= Time.deltaTime;
                Move();
                if(_timer < 0) {
                    _rb.velocity = Vector3.zero;
                    _timer = TIMER;
                    mob_anim.SetBool("walk", false);
                    mob_anim.SetBool("attack", false);
                    _curr_state = State.IDLE;
                }
                break;
            case State.ATTACKING:
                if(Vector3.Dot(_player.transform.position - transform.position, transform.right) > 0)
                    _sprite.flipX = false;
                if(Vector3.Dot( _player.transform.position - transform.position, transform.right) < 0)
                    _sprite.flipX = true;
                break;
        }
        if (!sounds[index].isPlaying && death) Destroy(this.gameObject);
    }
}
