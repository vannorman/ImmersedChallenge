using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
//using System.Linq;
//using System;

#if UNITY_EDITOR

using UnityEditor;


//[CustomEditor (typeof(HexGrid))]
public class EditorTools : Editor {



	// % command
	// ^ control
	// # option? shift?
	/* % – CTRL on Windows / CMD on OSX
	// # – Shift
	// & – Alt
	LEFT/RIGHT/UP/DOWN – Arrow keys
	F1…F2 – F keys
	HOME, END, PGUP, PGDN
	*/



	//	[MenuItem("Edit/Editor Tools/Rename Children")]
	//	public static void CombineChildren(){
	//		foreach(GameObject o in Selection.gameObjects){
	//			o.GetComponent<CombineChildren>().
	//		}

	[MenuItem("Edit/Editor Tools/Fix Height Of Spline")]
	public static void FixHeightOfSpline()
	{
		foreach (GameObject o in Selection.gameObjects)
		{
			RaycastHit hit = new RaycastHit();
			if (Physics.Raycast(o.transform.position, Vector3.down, out hit)) {
				Debug.Log("hit distance for " + o.name+": "+hit.distance);
				float idealHeight = 1;
				o.transform.position += -Vector3.up * (hit.distance - idealHeight);
			}

		}
	}


	[MenuItem("Edit/Editor Tools/Billboard max")]
	public static void BillboardMax(){
		foreach(GameObject o in Selection.gameObjects){
			o.GetComponent<Terrain>().treeBillboardDistance = 2000;
		}
	}

	[MenuItem("Edit/Editor Tools/Profiler Memory")]
	public static void ProfilerMemory(){
		Debug.Log("<color=#0f0> MEM:</color> get mono..");
		Debug.Log("heap:"+(UnityEngine.Profiling.Profiler.GetMonoHeapSize()/4096));
		Debug.Log("used:"+(UnityEngine.Profiling.Profiler.GetMonoUsedSize()/4096));
	//		Debug.Log("Get mono runtime memory size:"+UnityEngine.Profiling.Profiler.GetRuntimeMemorySize();
		Debug.Log("temp allocator:"+(UnityEngine.Profiling.Profiler.GetTempAllocatorSize()/4096));
		Debug.Log("total allocated:"+(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory()/4096));
		Debug.Log("total reserved:"+(UnityEngine.Profiling.Profiler.GetTotalReservedMemory()/4096));
	}



	[MenuItem("Edit/Editor Tools/Triangle Count")]
	public static void TriangleCount(){
		Debug.Log("<color=#0f0>"+Selection.activeGameObject.GetComponent<MeshFilter>().mesh.triangles.Length/3f+"</color> triangles");
	}



	static float scrollSensitivity = 50f; 
	[MenuItem("Edit/Editor Tools/Adjust Scroll View Sensitivity")]
	public static void AdjustScrollViewSensitivity(){
		scrollSensitivity += 50f;
		if (scrollSensitivity > 400) scrollSensitivity = 50;
		foreach(ScrollRect sr in Resources.FindObjectsOfTypeAll<ScrollRect>()){
			sr.scrollSensitivity = scrollSensitivity;
		}
	}



	[MenuItem("Edit/Editor Tools/Make Group %e")]
	public static void MakeGroup() {
		SaveScene();
		if (!Selection.activeGameObject) return;
		Undo.IncrementCurrentGroup();
		Undo.RegisterSceneUndo("Make Group");

		List<Object> toGroup = new List<Object>();
		foreach(GameObject g in Selection.gameObjects) {
			GameObject f = g;
	//			while(f.GetComponent<HexGridPiece>() == null && f.transform.parent != null) {
	//				f = f.transform.parent.gameObject;
	//			}
			toGroup.Add(f);
		}

	//		while (Selection.activeGameObject.name.Contains("(Clone)")){
	//			Selection.activeGameObject.name = 
	//		}
		GameObject newParent = new GameObject(Selection.activeGameObject.name + " group");
		Selection.objects = toGroup.ToArray();

		Transform root = Selection.activeTransform.parent;
		newParent.transform.parent = root;
		Vector3 center = Vector3.zero;
		foreach(GameObject g in Selection.gameObjects) {
			center += g.transform.position;
		}
		center /= Selection.gameObjects.Length;
		Vector3 middlestCenter = Vector3.zero;
		float closestDist = float.PositiveInfinity;
		foreach(GameObject g in Selection.gameObjects) {
			float d = (g.transform.position - center).sqrMagnitude;
			if(d < closestDist) {
				closestDist = d;
				middlestCenter = g.transform.position;
			}
		}
		newParent.transform.position = middlestCenter;
		newParent.transform.position = center;
		foreach(GameObject g in Selection.gameObjects) {
			g.transform.parent = newParent.transform;
		}	
		Selection.activeGameObject = newParent;

	}	

	//	[MenuItem("Edit/Editor Tools/Ungroup %#e")]
	public static void Ungroup() {
		Undo.IncrementCurrentGroup();
		Undo.RegisterSceneUndo("Ungroup");
		List<Object> newSel = new List<Object>();
		foreach(GameObject g in Selection.gameObjects) {
			while(g.transform.childCount > 0) {
				GameObject child = g.transform.GetChild(0).gameObject;
				newSel.Add(child);
				child.transform.parent = g.transform.parent;

			}
			DestroyImmediate(g);
		}
		Selection.objects = newSel.ToArray();
	}

	[MenuItem("Edit/Editor Tools/Count Vertex")]
	public static void CountVertex() {

		Debug.Log("vertex on "+Selection.activeObject.name+": "+Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.vertices.Length);

	}

	[MenuItem("Edit/Editor Tools/Count Collider")]
	public static void CountCollider() {
	//		// commented Debug.Log (FindObjectsOfType<Collider>().Length);
	//		// commented Debug.Log ("vertex on "+Selection.activeObject.name+": "+Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh.vertices.Length);

	}


	[MenuItem("Edit/Editor Tools/Draw Spheres On Verts")]
	public static void DrawSpheresOnVerts(){
		Mesh m = Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh;

		for (int i=0;i<m.vertices.Length;i++){
			Vector3 p = Selection.activeTransform.TransformPoint(m.vertices[i]);
			DebugSphere("vert " +i.ToString(),p).transform.parent = Selection.activeTransform;
		}
	}


	static GameObject DebugSphere(string n,Vector3 p){
		GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		DestroyImmediate (debugSphere.GetComponent<Collider>());
		debugSphere.transform.localScale = Vector3.one * .2f;
		debugSphere.name = n;
		debugSphere.transform.position = p;
		return debugSphere;
	}

	[MenuItem("Edit/Editor Tools/Debug CSV")]
	public static void DebugCSV() {
		string s="";
		foreach(Transform t in Selection.activeTransform){
			s += Regex.Match(t.name, @"\d+").Value +",";
	//			int i=0;
	//			if (int.TryParse(t.name,out i)){
	//				s += i.ToString();
	//			}
		}
		// commented Debug.Log (s);
	}

		static void SaveScene() {
			if (!Application.isPlaying)
			{
				EditorApplication.SaveScene();
			}
		}


	[MenuItem("Edit/Editor Tools/Select Parents %g")]
	public static void SelectParents() {
	//		DeleteAllHGP();
		SaveScene();
		List<Object> newSelection = new List<Object>();
		foreach(GameObject item in Selection.gameObjects) {
			Transform t = item.transform;
			//			Transform root = GameObject.Find(".gridRoot").transform;
			if(t.parent != null) {
				t = t.parent;
			}

			newSelection.Add(t.gameObject);
		}
		Selection.objects = newSelection.ToArray();
	}


	[MenuItem("Edit/Editor Tools/Rotate Ninety %#u")]
	public static void RotateSixty(){
		SaveScene();
		Quaternion rot = Selection.activeTransform.rotation;
		rot.eulerAngles += new Vector3(0,90,0);
		Selection.activeTransform.rotation = rot;
	}

	[MenuItem("Edit/Editor Tools/Rotate Sixty %u")]
	public static void RotateNegSixty(){
		SaveScene();
		foreach(Transform t in Selection.transforms){
			Quaternion rot = t.rotation;
			rot.eulerAngles += new Vector3(0,60,0);
			t.rotation = rot;
		}

	}

	[MenuItem("Edit/Editor Tools/Add Item Named Between %t")]
	public static void AddItemNamedBetween()
	{
			// For spline node addition
		var selectedName = Selection.activeGameObject.name;
		var origParent = Selection.activeGameObject.transform.parent;
		var origIndex = Selection.activeGameObject.transform.GetSiblingIndex();
		List<Transform> siblings = Selection.activeGameObject.transform.parent.GetComponentsInChildren<Transform>().Where(x => x != Selection.activeGameObject.transform.parent).ToList();
		siblings.OrderBy(x => x.name);
		bool push = false;
		foreach (var child in siblings) {
			if (child == Selection.activeGameObject.transform)
			{
				push = true;
			}
			if (child != Selection.activeGameObject.transform)
			{
				// skip all the children until the selected one
				if (push)
				{



					//var num = new Regex("(?<Alpha>[a-zA-Z]*)(?<Numeric>[0-9]*)");
					var numAlpha = new Regex("(?<Alpha>[a-zA-Z]*)(?<Numeric>[0-9]*)");
					var match = numAlpha.Match(child.name);

					var alpha = match.Groups["Alpha"].Value;					
					var num = match.Groups["Numeric"].Value;

					// How many digits?
					int zeroCount = num.Length;
					int newNum = (int)int.Parse(num) + 1;
					string newName = alpha + newNum.ToString().PadLeft(zeroCount,'0');

					Debug.Log("tried to push:" + child.name + " to become "+newName);
					child.name = newName;

				}
					//continue;
			}
			//if (child.name == Selection.activeGameObject.name) continue; // skip all the children until the selected one
		}

		GameObject newChild = (GameObject)Instantiate(Selection.activeGameObject,Selection.activeGameObject.transform.position,Selection.activeGameObject.transform.rotation);
		newChild.name = selectedName;
		newChild.transform.SetParent(origParent);
		newChild.transform.SetSiblingIndex(origIndex);
		Selection.activeGameObject = newChild;
	}


		[MenuItem("Edit/Editor Tools/Set Local Position To Zero %#y")]
	public static void SetLocalPositionToZero(){
		foreach(GameObject o in Selection.gameObjects){
			if (o.GetComponent<RectTransform>()){
				o.GetComponent<RectTransform>().localPosition = Vector3.zero;
			} else {
				o.transform.localPosition = Vector3.zero;
			}
		}
	}	

	static int posInt=0;
	[MenuItem("Edit/Editor Tools/Set Rotation To Zero2 %y")]
	public static void SetRotationToZero(){
		foreach(GameObject o in Selection.objects){
	//			Debug.Log("rot zero");
			switch (posInt % 4 ){
			case 0: 
				o.transform.localRotation = Quaternion.identity;
				break;
			case 1:
				Quaternion newRot2 = new Quaternion();
				newRot2.eulerAngles=new Vector3(270,0,0);
				o.transform.localRotation = newRot2;
				break;
			case 2:
				o.transform.rotation = Quaternion.identity;
				break;
			case 3:
				Quaternion newRot = new Quaternion();
				newRot.eulerAngles=new Vector3(270,0,0);
				o.transform.rotation = newRot;

				break;
			default:break;
			}
		}

		posInt++;
	}



	[MenuItem("Edit/Editor Tools/Select Children %#g")]
	public static void SelectChildren() {
		//		List<Object> newSel = new List<Object>();
		//		int maxSelect = 10000;
		//		foreach(Transform selT in Selection.transforms) {
		//			foreach(Transform t in selT) {
		//				newSel.Add(t.gameObject);
		//				if(maxSelect < 0) { break; }
		//				maxSelect--;
		//			}
		//		}
		//		Selection.objects = newSel.ToArray();
		GameObject newParent = new GameObject();
		newParent.transform.parent = Selection.activeTransform.parent;
		foreach(Transform t in Selection.transforms){
			t.parent = newParent.transform;
		}
	}



	static Color l_color;
	static float l_intensity;
	static float l_bounceIntensity;
	static LightShadows l_shadows = LightShadows.Hard;
	static Color a_color;
	static float a_intensity;
	//	static float 






	[MenuItem("Edit/Editor Tools/Save Lighting Values")]
	public static void SaveLightingValues() {
		Light l = GameObject.Find ("Directional light").GetComponent<Light>();
		l_color = l.color;
		l_intensity = l.intensity;
		l_bounceIntensity = l.bounceIntensity;
		a_color = RenderSettings.ambientLight;
		a_intensity = RenderSettings.ambientIntensity;
	}


	[MenuItem("Edit/Editor Tools/Fix Lighting")]
	public static void FixLightingInScenes() {
		SaveLightingValues();
		foreach(EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes){
			FixLighting (scene);
		}
	}

	static void FixLighting(EditorBuildSettingsScene scene){
		EditorApplication.OpenScene(scene.path);
		Light l = GameObject.Find ("Directional light").GetComponent<Light>();
		l.color = l_color;
		l.intensity = l_intensity;
		l.bounceIntensity = l_bounceIntensity;
		RenderSettings.ambientLight = a_color;
		RenderSettings.ambientIntensity = a_intensity;
	}


	[MenuItem("Edit/Editor Tools/Clear Trees")]
	public static void ClearTrees() {
		Terrain ter = Selection.activeGameObject.GetComponent<Terrain>();
		List<TreeInstance> tree = new List<TreeInstance>(ter.terrainData.treeInstances);
		tree.Clear();
		ter.terrainData.treeInstances = tree.ToArray();
	}

	[MenuItem("Edit/Editor Tools/Toggle Foggle &f")]
	public static void ToggleFoggle() {
		RenderSettings.fog = !RenderSettings.fog;
		// commented Debug.Log ("FOG:"+RenderSettings.fog);
	}


	[MenuItem("Edit/Terrain Generator/Drop To Terrain")]
	static float DropToTerrain(GameObject o){
		// grab verts

		Mesh mesh = o.GetComponent<MeshFilter>().mesh;
		Transform t = o.transform;

		Vector3[] vertices = mesh.vertices;

		float baseHeight = 0;
		for (int i = 0; i < vertices.Length; i++){
			foreach(RaycastHit hitt in Physics.RaycastAll(new Ray(t.TransformPoint(vertices[i]),Vector3.down))){
				if (hitt.collider.name == "plane"){
					if (hitt.distance > baseHeight) baseHeight = hitt.distance;
					break;
				}
			}
		}



		//		for (int i = 0; i < 20; i++){
		for (int i = 0; i < vertices.Length; i++){

			RaycastHit[] hits;
			Vector3 raypos = t.TransformPoint(vertices[i]);
			//			// commented Debug.Log("start ray at"+raypos);
			//			DebugSphere(raypos,i+"_raypos_start",t);
			hits = Physics.RaycastAll(new Ray(raypos,Vector3.down));
			foreach(RaycastHit hit in hits){

				if (hit.collider.transform.root != t.root) {
					//				if (hit.collider.name.Contains("plane")){
					// The distance from the vert to the ground is
					float dist = hit.distance;
					//					DebugSphere(hit.point,i+"_raypos_end",t);
					// todo unity bug hit distance sometimes gives SCALED distance? e.g. the localscale of this mesh is 5, hit distance will be x * 5. So fucking annoying. Only does it sometimes which is WORSE.
					dist = Vector3.Distance(hit.point,raypos);
					// distance from vert to center of mesh (shape)
					float diff = (vertices[i].z  - mesh.bounds.extents.z);
					// corrected dist and baseheight for scale of 5
					Vector3 result = vertices[i] - t.InverseTransformDirection(new Vector3(0,(dist-baseHeight)/5f-diff,0));
					// commented Debug.Log (i+": dist: " + dist+"; diff: " +diff +"l orig: "+vertices[i]+"; result:" + result);
					vertices[i] = result;

					//					vertices[i] = t.InverseTransformPoint(hit.point + diff + baseHeight);
					break;
				}
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();

		float minHeight = Mathf.Infinity;
		for (int i = 0; i < vertices.Length; i++){
			foreach(RaycastHit hitt in Physics.RaycastAll(new Ray(t.TransformPoint(vertices[i]),Vector3.down))){
				if (hitt.collider.name == "plane"){
					if (hitt.distance < minHeight) minHeight = hitt.distance;
					break;
				}
			}
		}

		//		float baseHeight = 0;
		for (int i = 0; i < vertices.Length; i++){
			foreach(RaycastHit hitt in Physics.RaycastAll(new Ray(t.TransformPoint(vertices[i]),Vector3.down))){
				if (hitt.collider.name == "plane"){
					if (hitt.distance > baseHeight) baseHeight = hitt.distance;
					break;
				}
			}
		}
		return baseHeight;


		// commented Debug.Log ("baseheight (max,min):"  +baseHeight+","+minHeight+" on "+t.name);
		return minHeight;
	}



	[MenuItem("Edit/Editor Tools/Fix Buttons")]
	public static void FixButtons() {
		foreach(Button b in FindObjectsOfType<Button>()){
			b.transition = Button.Transition.ColorTint;
		}
	}


	[MenuItem("Edit/Editor Tools/Terrain Material")]
	public static void TerrainMaterial() {

		foreach(Terrain t in FindObjectsOfType<Terrain>()){
			t.materialType = Terrain.MaterialType.BuiltInLegacyDiffuse;
		}
	}








	//	static bool bold = false;
	[MenuItem("Edit/Editor Tools/Set Font Aspira")]
	public static void SetFontAspira() {

	//		bold = !bold;
		foreach(GameObject o in Selection.gameObjects){
			Text t = o.GetComponent<Text>();
			if (t){
				t.font = (Font)Resources.Load("Durotype - Aspira-Medium hinted sharp (gui)");
	//				t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; 
				t.fontStyle = FontStyle.Normal;
				t.gameObject.transform.position += Vector3.one;
				t.gameObject.transform.position -= Vector3.one;
				t.transform.localScale = Vector3.one * 0.5f;
				t.fontSize = t.transform.parent.localScale.x == 1 ? 28 : 100;

			}
		}

	}



	[MenuItem("Edit/Editor Tools/Replace Arial Fonts")]
	public static void ReplaceArialFonts() {
		foreach(Text t in Resources.FindObjectsOfTypeAll<Text>()){
	//			if (t.font.ToString().Contains("Arial") ){ //|| t.font.ToString().Contains("OpenSans")){
	//			if (t.font.ToString().Contains("Candara")){
			t.font = (Font)Resources.Load("Fonts/Durotype - Aspira-Medium hinted sharp (gui)") as Font;
			t.name += " moveme? ";
	//				// commented Debug.Log("font:"+t.font+" on "+t.gameObject.name);
		}

	}

	[MenuItem("Edit/Editor Tools/Fix Font Size")]
	public static void FixFontSize() {
		foreach(Text t in Resources.FindObjectsOfTypeAll<Text>()){

			if (t.fontSize == 28  && t.transform.localScale.x == 0.5f){
				t.fontSize = 19;
				t.transform.localScale = Vector3.one * 0.75f;
			}
	//			t.name += " moveme? ";
			//				// commented Debug.Log("font:"+t.font+" on "+t.gameObject.name);
		}

	}

	[MenuItem("Edit/Editor Tools/Remove moveme _m")]
	public static void RemoveMoveMe(){
		if (Selection.activeGameObject.name.Contains("moveme?")){
			string rep = Selection.activeGameObject.name.Replace("moveme?","");
			Selection.activeGameObject.name = rep;
		}
	}



	[MenuItem("Edit/Editor Tools/Toggle Gameobject Active _a")]
	public static void ToggleGameObjectActive()
	{
			if (Application.isPlaying) return;
		SaveScene();
		bool f = Selection.activeGameObject.activeSelf;
		foreach(GameObject o in Selection.gameObjects){
			o.SetActive(!f);
		}
	}

	[MenuItem("Edit/Editor Tools/Set Vector3.one Scale _s")]
	public static void SetVector3OneScale()
	{
		SaveScene();
		foreach(GameObject o in Selection.gameObjects){
			o.transform.localScale = Vector3.one;
		}
	}

	static float scaleMod = 0.5f;
	[MenuItem("Edit/Editor Tools/Set alt Scale _#s")]
	public static void SetAltScale()
	{
		SaveScene();
		foreach(GameObject o in Selection.gameObjects){
			o.transform.localScale = Vector3.one * scaleMod;
		}
		scaleMod = scaleMod == 0.5f ? 0.25f : 0.5f;
	}

	[MenuItem("Edit/Editor Tools/Clear Player Prefs")]
	public static void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}


	[MenuItem("Edit/Editor Tools/Find Empty Meshes")]
	public static void FindEmptyMeshes() {
		foreach(MeshFilter m in FindObjectsOfType<MeshFilter>()){
			if (m.sharedMesh == null){
				// commented Debug.Log("m:"+m.name);
			}
		}

		foreach(SkinnedMeshRenderer m in FindObjectsOfType<SkinnedMeshRenderer>()){
	//			if (m.shared == null){
				// commented Debug.Log("m:"+m.name);
	//			}
		}
	}


	[MenuItem("Edit/Editor Tools/Calculate Radial")]
	public static void CalculateRadial()
	{
		Transform transform = Selection.activeTransform;
		if (transform.childCount == 0)
			return;
		float MinAngle = 0;
		float MaxAngle = 360;
		float fOffsetAngle = ((MaxAngle - MinAngle)) / (transform.childCount);
		float fDistance = 80;
		float fAngle = 0;
		for (int i = 0; i < transform.childCount; i++)
		{
			RectTransform child = (RectTransform)transform.GetChild(i);
			if (child != null)
			{
				//Adding the elements to the tracker stops the user from modifiying their positions via the editor.
				Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
				child.localPosition = vPos * fDistance;
				//Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
				child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
				fAngle += fOffsetAngle;
			}
		}

	}


	[MenuItem("Edit/Editor Tools/Create Circle Of Points")]
	public static void CreateCircleOfPoints()
	{
		if (Selection.activeGameObject == null) return;
		foreach (var p in Utils2.GetCircleOfPoints(360,12,5)) {
			var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
			c.transform.position = Selection.activeGameObject.transform.position + p;
			c.transform.LookAt(Selection.activeGameObject.transform);
			c.transform.SetParent(Selection.activeGameObject.transform);
		}
	}

	[MenuItem("Edit/Editor Tools/Build Ramps Around Spline")]
	public static void BuildRampsAroundSpline()
	{
		Debug.Log("start");
		// orient self at spline 0 pointing toward spline node 1
		var nodes = Selection.activeGameObject.GetComponentsInChildren<Transform>().OrderBy(x => x.name).ToList();
		var p = nodes[0].position;
		var dir = nodes[1].position - nodes[0].position;
		var carat = new GameObject();
		carat.transform.position = p;
		carat.transform.LookAt(nodes[1]);

		// look for right edge near spline up to 5 units away
		var maxRadius = 10f;
		var hit = new RaycastHit();
		int currentNode = 0;
		Debug.Log("start ray");
		if (Physics.Raycast(p, Vector3.down, out hit)) {
			Debug.Log("ray suc");
			var baseDist = hit.distance;
			var deltaDistToBuildWall = 0.5f;
			Vector3 centerPoint = hit.point;
			var hitPoint = centerPoint;
			float stepSize = 0.1f;
			float yScale = 5f;
			Vector3 lastHitPoint = Vector3.zero;
			float xScale = 15f;
			bool foundRight = false;
			var groundColliderName = hit.collider.name;
			// Look right
			for (var i = 0f; i < maxRadius; i += stepSize) {

				if (Physics.Raycast(carat.transform.position, Vector3.down, out hit)) {

					lastHitPoint = hit.point;
					float deltaDist = Mathf.Abs(hit.distance - baseDist);
					if (deltaDist > deltaDistToBuildWall || hit.collider.name != groundColliderName) { // if the delta was too high (fell off) or name was different from previous ground (fell off)
						GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
						Debug.Log("Hit wall! RIGHT. Hit dist:"+hit.distance+", base dist:"+baseDist);
						wall.transform.position = hit.point;
						wall.transform.LookAt(nodes[currentNode]);
						wall.transform.localScale = new Vector3(xScale, yScale, 0.1f);
						wall.name = "ramp wall RIGHT at "+currentNode;
						Debug.Log("wal name:"+wall.name);
						foundRight = true;
						break;

					} else {
						Debug.Log("deltaDist:"+deltaDist);
						hitPoint = hit.point;
						carat.transform.position =  nodes[currentNode].position + carat.transform.right * i;
					}
				}
			}

			if (!foundRight) {
				Debug.Log("Never found right! Placing ..");
				GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
				wall.transform.position = hit.point;
				wall.transform.LookAt(nodes[currentNode]);
				wall.transform.localScale = new Vector3(xScale, yScale, 0.1f);
				wall.name = "ramp wall RIGHT at " + currentNode;
			}

			// Look left
			carat.transform.position = nodes[currentNode].position;
			for (var i = 0f; i < maxRadius; i += stepSize)
			{
				if (Physics.Raycast(carat.transform.position, Vector3.down, out hit))
				{
					if (Mathf.Abs(hit.distance - baseDist) > deltaDistToBuildWall || hit.collider.name != groundColliderName)
					{
						GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
						Debug.Log("Hit wall! LEFT");
						wall.transform.position = hit.point;
						wall.name = "ramp wall LEFT at " + currentNode;
						wall.transform.LookAt(nodes[currentNode]);
						wall.transform.localScale = new Vector3(yScale,yScale,0.1f);
						break;

					}
					else
					{
						hitPoint = hit.point;
						carat.transform.position = nodes[currentNode].position + -carat.transform.right * i;
					}
				}
			}

			DestroyImmediate(carat);
			// spline is above ground
		} else {
			Debug.Log("<color=#f55>FAIL</color> to find ground in spline raycast");
		}


	}


	[MenuItem("Edit/Editor Tools/Generate Terrain Skirt")]
	public static void GenerateTerrainSkirt(){
		Terrain at = Selection.activeGameObject.GetComponent<Terrain>();

	//		Terrain.activeTerrain.terrainData.GetHeights(
	//		Vector3[] verts = Terrain.activeTerrain.terrainData.GetHeights(
		TerrainData myTerrainData = at.terrainData;
		int res = at.terrainData.heightmapResolution;
		float scalexz = at.terrainData.size.x;
		float scaley = at.terrainData.size.y;
		float[,] allheights = myTerrainData.GetHeights(0,0,res,res);

		for (int i=0;i<allheights.GetLength(0);i++){
			for (int j=0;j<allheights.GetLength(1);j++){
	//				// commented Debug.Log("allehgitsh "+i+","+j+": "+allheights[i,j]);
				if (i < 2 || i > allheights.GetLength(0) - 3 || j < 2 || j > allheights.GetLength(1) - 3) {
					allheights[i,j] = 0;
				}
			}
		}
		myTerrainData.SetHeights(0,0,allheights);
		return;


	}


	[MenuItem("Edit/Editor Tools/Generate Terrain Ramp #r")]
	public static void GenerateTerrainRamp(){
		GameObject o1 = Selection.activeGameObject;
		MeshFilter mf = o1.GetComponent<MeshFilter>();
		Mesh m = mf.sharedMesh;
		for (int i=0;i<m.vertices.Length;i++){
			TerrainData myTerrainData = Terrain.activeTerrain.terrainData;
			int res = Terrain.activeTerrain.terrainData.heightmapResolution;
			Vector3 hScale = Terrain.activeTerrain.terrainData.heightmapScale;
			float sizeX = Terrain.activeTerrain.terrainData.size.x;
			float sizeZ = Terrain.activeTerrain.terrainData.size.z;
	//			// commented Debug.Log("res:"+res+", scale:"+hScale+", hw;"+sizeX+","+sizeZ);
			Vector3 worldpos = o1.transform.TransformPoint(m.vertices[i]);
	//			// commented Debug.Log("worldpos:"+worldpos);
			int xStart = Mathf.FloorToInt(worldpos.x * res / sizeX);
			int zStart = Mathf.FloorToInt(worldpos.z * res / sizeZ);
	//			// commented Debug.Log("xst:"+xStart+","+zStart);
			float[,] heights = myTerrainData.GetHeights(xStart,zStart,1,1);
	//			// commented Debug.Log("xz:"+xStart+","+zStart);
	//			// commented Debug.Log("heights:"+heights+", len:"+heights.GetLength(0)+","+heights.GetLength(1));
			for (int j=0;j<heights.GetLength(0);j++){
				for (int k=0;k<heights.GetLength(1);k++){
					heights[j,k] = worldpos.y / hScale.y;
				}
			}
	//			float[,] heights = new float[0f,worldpos.y / myTerrainData.detailHeight];
			myTerrainData.SetHeights(xStart,zStart,heights);
	//			// commented Debug.Log("height "+i+": "+height);

		}




	}




	[MenuItem("Edit/Editor Tools/Set Tree Billboard Dist")]
	public static void SetTreeBillboardDist()
	{
		foreach(Terrain t in FindObjectsOfType<Terrain>()){
			t.treeBillboardDistance = 500;

			t.treeCrossFadeLength = 40;
			t.detailObjectDistance = 80;

		}

	}

	[MenuItem("Edit/Editor Tools/Get Compressed Size")]
	public static void GetCompressedFileSize()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (!string.IsNullOrEmpty(path))
		{
			string guid = AssetDatabase.AssetPathToGUID(path);
			string p = Path.GetFullPath(Application.dataPath + "../../Library/cache/" + guid.Substring(0, 2) + "/" + guid);
			if (File.Exists(p))
			{
				var file = new FileInfo(p);
				// commented Debug.Log("Compressed file size of " + path + ": " + file.Length + " bytes");
			}
			else
			{
				// commented Debug.Log("No file found at: " + p);
			}
		}
	}
















}
#endif




