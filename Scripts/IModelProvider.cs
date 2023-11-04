using System;
using UnityEngine;

namespace WildPerception
{

    public abstract class AbstractPedestrianModelProvider: MonoBehaviour
    {
        public abstract GameObject GetPedestrianModel();
    }
}