using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the movement of a platform that moves between several predefined points in space.
public class MovingPlatform : MonoBehaviour
{
    public List<Transform> points; // List of Transform points the platform will move between
    public Transform platform;     // The platform GameObject itself (assigned in the editor) that will move between the points.
    int goalPoint = 0;             // An integer tracking the current target point (or "goal") the platform is moving towards.
    public float moveSpeed = 2;    // The speed at which the platform will move between points. 

    // Update is called once per frame
    void Update()
    {
        // Move the platform towards the next point in the list.
        MoveToNextPoint();
    }

    // Moves the platform towards the next goal point in the list.
    void MoveToNextPoint(){
        /*
            The Vector2.MoveTowards method in Unity is used to move a point in 2D space towards a target point at a specified speed. 
            It takes three parameters:
            - platform.position: The current position of the platform.
            - points[goalPoint].position: The target position the platform is moving towards.
            - Time.deltaTime * moveSpeed: The distance to move in this frame, based on the specified movement speed.

            Time.deltaTime: time (in seconds) that has passed since the last frame.
        */
        platform.position = Vector2.MoveTowards(platform.position, points[goalPoint].position, Time.deltaTime * moveSpeed);

        // Check if the platform has reached the target point (goalPoint).
        if(Vector2.Distance(platform.position, points[goalPoint].position) < 0.1f){
            /*
                Checks if the platform has reached the last point in the list.
                - If true, the goalPoint is reset to 0, making the platform return to the first point, creating a loop.
                - Otherwise, goalPoint increments to the next point in the list.
            */
            if(goalPoint == points.Count - 1){
                goalPoint = 0; // Loop back to the first point if we've reached the last one.
            }
            else{
                goalPoint++; // Move to the next point in the list.
            }
        }
    }
}
