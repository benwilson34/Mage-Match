using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GFX {
    class DestroyParticles : MonoBehaviour {

        private ParticleSystem ps;

        public void Start() {
            ps = GetComponent<ParticleSystem>();
        }

        //public void Update() {
        //    if (!ps.IsAlive())
        //        Destroy(this.gameObject);
        //}

        IEnumerator DestroyAfterLifetime() {
            yield return new WaitForSeconds(ps.main.duration);
            Destroy(this.gameObject);
        }
    }
}
