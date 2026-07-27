using System;
using UnityEngine;

namespace Enemies
{
    public class Enemy : MonoBehaviour
    {
        const float arrivalThreshold = 0.05f;

        Transform target;
        float moveSpeed;
        bool isActive;
    
        Action<Enemy> despawnCallback;
    
        public void Configure(Action<Enemy> despawnCallback, Transform target, float moveSpeed)
        {
            this.despawnCallback = despawnCallback;
            this.target = target;
            this.moveSpeed = moveSpeed;
            isActive = true;
        }

        void Update()
        {
            if (!isActive || target == null)
                return;

            Vector3 destination = target.position;
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination) <= arrivalThreshold)
            {
                isActive = false;
                despawnCallback?.Invoke(this);
            }
        }
    }
}
