var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
return "clip=" + p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>().CurrentClip;
