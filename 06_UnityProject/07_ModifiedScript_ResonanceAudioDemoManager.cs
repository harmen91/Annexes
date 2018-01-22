// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


// Additional functionality by Harmen Smit

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

/// Class that manages the ResonanceAudioDemo scene.
public class ResonanceAudioDemoManager : MonoBehaviour {

 /// cube counting variables
  public int cubeCounter = 0;
  public int maxCubes;

  /// Main camera.
  public Camera mainCamera;

  // Time of latest press
  public double pressTime;

  // Where to write the CSV, set path in unity editor
  public string csv_file_path;

  // Visibility Detection with a collider
  public Plane[] planes;
  public Collider anObjCollider;

  /// time variables
  List<double> searchTimes = new List<double>();
  List<bool> is_visibles = new List<bool>();

  /// Cube controller.
  public ResonanceAudioDemoCubeController cube;

  void Start() {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    // Initilize current time
    pressTime = get_cur_time();
    // Collision
    planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
  }


  void Update() {
#if !UNITY_EDITOR
    if (Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
#endif  // !UNITY_EDITOR
    // Raycast against the cube.
    Ray ray = mainCamera.ViewportPointToRay(0.5f * Vector2.one);
    RaycastHit hit;
    bool cubeHit = Physics.Raycast(ray, out hit) && hit.transform == cube.transform;
    // Update the state of the cube.
    cube.SetGazedAt(cubeHit);
    if (cubeHit) {
      if(Input.GetKeyDown("joystick button 1")) {
        // Teleport the cube to its next location.
        cube.TeleportRandomly();
        // Count the cubes
        cubeCounter+=1;
        // True / False for collision from camera-viewplane with object (aka Visibility)
        bool is_visible = GeometryUtility.TestPlanesAABB(planes, anObjCollider.bounds);
        // is visibles is a list, here adding is_visible
        is_visibles.Add(is_visible);
        // 64 bits integer, /1000 to make seconds out of milliseconds
        double searchTime = (get_cur_time() - pressTime)/1000;
        // searchTimes is a list, here it adds seachTime to the list
        searchTimes.Add(searchTime);
        // change pressTime to current time for the next cube
        pressTime = get_cur_time();
        // checkpoint for writing CSV file
        if (cubeCounter == maxCubes) {
          stop_scene();
        }
      }
    }
  }
  // function to get time, put it in a 64 bit integer for milliseconds
  double get_cur_time(){
    return (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
  }

// writing to csv
void stop_scene(){
   var csv = new StringBuilder();
   for (int i = 1; i < maxCubes; i++) //start at the second cube, since the first one doesnt have a visibility colider boolean value
        {
            csv.AppendLine(searchTimes[i].ToString()+", " + is_visibles[i-1]); //-1 to shift the visibility factor to the next line on the csv corresponding with the right cube
        }
   File.WriteAllText(csv_file_path, csv.ToString());
   // Crash application with some random function after writing text to csv file -hueheuheuheu-, because Application.Quit() is not available in unity editor mode, and I'm planning to test with the editor open, for changing csv paths
   // double j = searchTimes[200000];
}

}
