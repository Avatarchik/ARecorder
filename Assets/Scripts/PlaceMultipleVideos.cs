﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlaceMultipleVideos : MonoBehaviour {
	public List<Row> list;
	public List<GameObject> videoPlayerList = new List<GameObject>();
	public TimelineController timelineController;
	public List<ImagetargetPositionInfo> allImagetargetInfo;
	private List<GameObject> arrows = new List<GameObject>();
	private List<float> arrowsToOriginDistance = new List<float> ();
	public GameObject videoPlayerPrefab;
	public GameObject arrowPrefab;
	public ImagetargetPositionInfo currentImagetargetInfo;
	private Vector3 initPos = Vector3.zero;
	// Use this for initialization
	IEnumerator Start () {
		PersistentStorage.init();
		ReadImagetargetPosition.init ();
		list = PersistentStorage.findForImageTargetId(gameObject.name);
		currentImagetargetInfo = ReadImagetargetPosition.findForImageTargetPosition (gameObject.name);
		allImagetargetInfo = ReadImagetargetPosition.getAllImagetargerPositionInfo();
		foreach (Row row in list) {
			Vector3 shrinkedPos = new Vector3(row.localPosition.x / 3.0f,
											  row.localPosition.y / 3.0f,
											  row.localPosition.z / 3.0f);
			Vector3 position = transform.TransformPoint(shrinkedPos);
			Quaternion rotation = transform.rotation * row.localRotation;
			GameObject videoPlayer = Instantiate(videoPlayerPrefab, transform);
			videoPlayerList.Add(videoPlayer);
			videoPlayer.transform.position = position;
			videoPlayer.transform.rotation = rotation;
			GameObject videoPlane = videoPlayer.transform.Find("VideoPlane").gameObject;
			TextMesh videoName = videoPlayer.transform.Find("VideoName").GetComponent<TextMesh>();
			videoName.text = row.videoPath;
			VideoPlayer player = videoPlane.GetComponent<VideoPlayer>();
			player.url = Application.persistentDataPath + "/" + row.videoPath;
			Debug.LogFormat("Row: {0} {1}", row.imageTargetId, row.videoPath);
			player.Play();
			timelineController.rowVideoList.Add(new RowVideoPair(row, videoPlayer));
		}

		foreach (ImagetargetPositionInfo info in allImagetargetInfo) {
			if (info.imageTargetId == this.gameObject.name) {
				initPos = new Vector3 (info.x, info.y, info.z);
				arrows.Add (null);
				arrowsToOriginDistance.Add(0.0f);
				Debug.LogFormat("Image Target: {0} Pos: {1}", info.imageTargetId, initPos);
			} else {
				GameObject newArrow = GameObject.Instantiate (arrowPrefab, transform);
				TextMesh pointedImage = newArrow.transform.Find ("pointedImage").GetComponent<TextMesh>();
				pointedImage.text = info.imageTargetId;
				float distance = Vector3.Distance (transform.position, newArrow.transform.position);
				arrows.Add (newArrow);
				arrowsToOriginDistance.Add (distance);
			}
		}
		Vector3 curPos = transform.position;
		for (int i = 0; i < allImagetargetInfo.Count; i++) {
			if (arrows [i] != null) {
				float radians = Mathf.Atan2(initPos.z - allImagetargetInfo[i].z, initPos.x - allImagetargetInfo[i].x);
				arrows[i].transform.eulerAngles = new Vector3(0, radians * 180 / Mathf.PI, 0);
				arrows[i].transform.position = new Vector3(curPos.x - arrowsToOriginDistance[i] * Mathf.Cos(radians),
															arrows[i].transform.position.y,
															curPos.z - arrowsToOriginDistance[i] * Mathf.Sin(radians));
			}
		}
		yield return new WaitForSeconds(1f); //wait the first frame to show up
		foreach (GameObject videoPlayer in videoPlayerList) {
			GameObject videoPlane = videoPlayer.transform.Find("VideoPlane").gameObject;
			VideoPlayer player = videoPlane.GetComponent<VideoPlayer>();
			player.Pause();
			int videoWidth = player.texture.width;
			int videoHeight = player.texture.height;
			Debug.LogFormat("Video width: {0} height: {1}", videoWidth, videoHeight);
			videoPlane.transform.localScale = new Vector3(videoPlane.transform.localScale.x,
															videoPlane.transform.localScale.y,
															videoPlane.transform.localScale.z * videoHeight / videoWidth);
		}
	}
}
