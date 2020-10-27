using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEditor;

using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SimpleFileBrowser;
using System;

public class Data
{
	public readonly static Data Instance = new Data();

	public string referer = string.Empty;
	public List<GameObject> alives_cp;

}