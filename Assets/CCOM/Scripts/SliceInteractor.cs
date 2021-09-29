//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Demonstrates how to create a simple interactable object
//
//=============================================================================

using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;


//-------------------------------------------------------------------------
[RequireComponent( typeof( Interactable ) )]
public class SliceInteractor : MonoBehaviour
{
    private Vector3 oldPosition;
	private Quaternion oldRotation;

	private Vector3 initialPositionalOffset;
	private Quaternion initialRotationalOffset;


	private Transform sliceToHand;

	private float attachTime;

	private Hand.AttachmentFlags attachmentFlags =
		Hand.defaultAttachmentFlags &
		(~Hand.AttachmentFlags.SnapOnAttach) &
		(~Hand.AttachmentFlags.DetachOthers) &
		(~Hand.AttachmentFlags.VelocityMovement) & 
		(~Hand.AttachmentFlags.ParentToHand);

    private Interactable interactable;

	//-------------------------------------------------
	void Awake()
	{
        interactable = this.GetComponent<Interactable>();
		interactable.hideHandOnAttach = false;
		interactable.handFollowTransform = false;
	}


	//-------------------------------------------------
	// Called when a Hand starts hovering over this object
	//-------------------------------------------------
	private void OnHandHoverBegin( Hand hand )
	{
		
	}


	//-------------------------------------------------
	// Called when a Hand stops hovering over this object
	//-------------------------------------------------
	private void OnHandHoverEnd( Hand hand )
	{
		
	}


	//-------------------------------------------------
	// Called every Update() while a Hand is hovering over this object
	//-------------------------------------------------
	private void HandHoverUpdate( Hand hand )
	{
        GrabTypes startingGrabType = hand.GetGrabStarting();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
        {
            // Save our position/rotation so that we can restore it when we detach
            oldPosition = transform.position;
            oldRotation = transform.rotation;

            // Call this to continue receiving HandHoverUpdate messages,
            // and prevent the hand from hovering over anything else
            hand.HoverLock(interactable);

            // Attach this object to the hand
            hand.AttachObject(gameObject, startingGrabType, attachmentFlags);			
		}
        else if (isGrabEnding)
        {
            // Detach this object from the hand
            hand.DetachObject(gameObject);

            // Call this to undo HoverLock
            hand.HoverUnlock(interactable);

            // Restore position/rotation
            //transform.position = oldPosition;
            //transform.rotation = oldRotation;
        }
	}


	//-------------------------------------------------
	// Called when this GameObject becomes attached to the hand
	//-------------------------------------------------
	private void OnAttachedToHand( Hand hand )
    {
		Transform handAttachmentPointTransform = hand.transform;
		initialPositionalOffset = handAttachmentPointTransform.InverseTransformPoint(gameObject.transform.position);
		initialRotationalOffset = Quaternion.Inverse(handAttachmentPointTransform.rotation) * gameObject.transform.rotation;

		attachTime = Time.time;
	}



	//-------------------------------------------------
	// Called when this GameObject is detached from the hand
	//-------------------------------------------------
	private void OnDetachedFromHand( Hand hand )
	{
        
	}


	//-------------------------------------------------
	// Called every Update() while this GameObject is attached to the hand
	//-------------------------------------------------
	private void HandAttachedUpdate( Hand hand )
	{
		Quaternion handRot = hand.transform.rotation;
		gameObject.transform.position = hand.transform.TransformPoint(initialPositionalOffset);
		gameObject.transform.rotation = handRot * initialRotationalOffset;
	}

    private bool lastHovering = false;
    private void Update()
    {
        if (interactable.isHovering != lastHovering) //save on the .tostrings a bit
        {
            
            lastHovering = interactable.isHovering;
        }
    }


	//-------------------------------------------------
	// Called when this attached GameObject becomes the primary attached object
	//-------------------------------------------------
	private void OnHandFocusAcquired( Hand hand )
	{
	}


	//-------------------------------------------------
	// Called when another attached GameObject becomes the primary attached object
	//-------------------------------------------------
	private void OnHandFocusLost( Hand hand )
	{
	}
}
