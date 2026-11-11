var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
return "dodging=" + p.GetComponent<Reborn.Player.PlayerController>().IsDodging + " clip=" + p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>().CurrentClip;
