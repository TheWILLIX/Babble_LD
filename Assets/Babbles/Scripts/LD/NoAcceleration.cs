using ClemCAddons.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoAcceleration : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        var dot = Vector3.Dot(Vector3.forward, collision.GetContact(0).normal);
        if (collision.transform.tag == "Player" && dot > 0.5f || dot < -0.5f)
        {
            collision.transform.GetComponent<CharacterMovement>().SetWallSlide(true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        float dot = Vector3.Dot(transform.up, collision.GetContact(0).normal);
        if (collision.transform.tag == "Player" && dot < 0.5f && dot > -0.5f)
        {
            collision.transform.GetComponent<CharacterMovement>().SetWallSlide(true);
        }
        else
        {
            collision.transform.GetComponent<CharacterMovement>().SetWallSlide(false);
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if(collision.transform.tag == "Player")
        {
            collision.transform.GetComponent<CharacterMovement>().SetWallSlide(false);
        }
    }
}
