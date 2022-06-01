using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VisualisationModel
    {
    public string DTName { get; set; }
    public GameObject Visual { get; set; }
    public GameObject VisualInfo { get; set; }
    public DataModel Data { get; set; }
    public float InitialVisualScale { get; set; }
    public Vector3 InitialInfoScale { get; set; }
}
