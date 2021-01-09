using System;
using System.Collections.Generic;
using UnityEngine;


/* Spawn object if bumped by Player's head
 * Applicable to: Collectible brick and question blocks
 */

public class CollectibleBlock : MonoBehaviour, ITimelined {
	private Animator m_Animator;
	private LevelManager t_LevelManager;

	public bool isPowerupBlock;
	public GameObject objectToSpawn;
	public GameObject bigMushroom;
	public GameObject fireFlower;
	public int timesToSpawn = 1;
	public Vector3 spawnPositionOffset;

	private float WaitBetweenBounce = .25f;
	private bool isActive;
	private float time1, time2;

	public Timeline timeline;

	public List<GameObject> enemiesOnTop = new List<GameObject> ();

    public bool Replaying { get; private set; }

    // Use this for initialization
    void Start () {
		m_Animator = GetComponent<Animator> ();
		t_LevelManager = FindObjectOfType<LevelManager> ();
		time1 = Time.time;
		isActive = true;

		if (timeline == null)
			throw new Exception($"'{gameObject.name}': timeline is null");
		timeline.OnInverted += OnTimelineInverted;
	}

    private void OnTimelineInverted(int direction)
    {
		Replaying = true;
    }

    void OnTriggerEnter2D(Collider2D other) {
		time2 = Time.time;
		if (other.tag == "Player" && time2 - time1 >= WaitBetweenBounce && !other.gameObject.GetComponent<Mario>().Replaying) {
			t_LevelManager.soundSource.PlayOneShot (t_LevelManager.bumpSound);

			if (isActive) {
				m_Animator.SetTrigger ("bounce");

				new AnimationSnapshot(
					owner: this,
					direction: 0,
					trigger: "bounce")
				._(timeline.Record);


				// Hit any enemy on top
				foreach (GameObject enemyObj in enemiesOnTop) {
					t_LevelManager.BlockHitEnemy (enemyObj.GetComponent<Enemy> ());
				}

				if (timesToSpawn > 0) {
					if (isPowerupBlock) { // spawn mushroom or fireflower depending on Mario's size
						if (t_LevelManager.marioSize == 0) {
							objectToSpawn = bigMushroom;
						} else {
							objectToSpawn = fireFlower;
						}
					}
					var position = transform.position + spawnPositionOffset;
					var instance = Instantiate(objectToSpawn, position, Quaternion.identity).GetComponentInChildren<Animator>();
					new CoinSnapshot(
						owner: this,
						direction: timeline.Direction,
						forward: true,
						position: position,
						quaternion: Quaternion.identity)
					._(timeline.Record);
					new CoinSnapshot(
						owner: this,
						direction: -1 * timeline.Direction,
						forward: false,
						position: position,
						quaternion: Quaternion.identity)
					._(_ => timeline.Record(_, instance.GetCurrentAnimatorStateInfo(0).length + instance.GetNextAnimatorStateInfo(0).length));
					var x = instance.GetCurrentAnimatorStateInfo(0).length + instance.GetNextAnimatorStateInfo(0).length;
					Debug.Log($"Look at that: {x}");

					timesToSpawn--;

					if (timesToSpawn == 0) {
						m_Animator.SetTrigger ("deactivated");

						new AnimationSnapshot(
							owner: this,
							direction: timeline.Direction,
							trigger: "deactivated")
						._(timeline.Record);

						new AnimationSnapshot(
							owner: this,
							direction: -1 * timeline.Direction,
							trigger: "activated")
						._(_ => timeline.Record(_, m_Animator.GetCurrentAnimatorStateInfo(1).length + m_Animator.GetNextAnimatorStateInfo(1).length));

						isActive = false;
					}			
				}
			}

			time1 = Time.time;
		}
	}


	// check for enemy on top
	void OnCollisionStay2D(Collision2D other) {
		Vector2 normal = other.contacts[0].normal;
		Vector2 topSide = new Vector2 (0f, -1f);
		bool topHit = normal == topSide;
		if (other.gameObject.tag.Contains("Enemy") && topHit && !enemiesOnTop.Contains(other.gameObject)) {
			enemiesOnTop.Add(other.gameObject);
		}
	}

	void OnCollisionExit2D(Collision2D other) {
		if (other.gameObject.tag.Contains("Enemy")) {
			enemiesOnTop.Remove (other.gameObject);
		}
	}

    public void Play(ISnapshot snapshot)
    {
		if (!Replaying) return;

		if (snapshot is AnimationSnapshot animation)
        {
			m_Animator.SetTrigger(snapshot.As<AnimationSnapshot>().Trigger);
		}
		else if (snapshot is CoinSnapshot coin)
		{
			Debug.Log($"{DateTime.Now.ToShortTimeString()}: playin");

			var instance = Instantiate(objectToSpawn, coin.Position, coin.Quaternion);
			instance.GetComponentInChildren<Animator>().SetBool("forward", coin.Forward);
		}
		else throw new Exception($"Unknown snapshot type: {snapshot.GetType()}");
    }

    private sealed class AnimationSnapshot : BaseSnapshot
    {
		public string Trigger { get;  }

        public AnimationSnapshot(ITimelined owner, string trigger, int direction) : base(owner)
        {
			Trigger = trigger;
			Direction = direction;
        }
    }

    private sealed class CoinSnapshot : BaseSnapshot
    {
        public bool Forward { get; }
		public Vector3 Position { get; }
		public Quaternion Quaternion { get; }

		public CoinSnapshot(ITimelined owner, int direction, bool forward, Vector3 position, Quaternion quaternion) : base(owner)
        {
			Forward = forward;
			Direction = direction;
			Position = position;
			Quaternion = quaternion;
        }
    }
}
