using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleController : MonoBehaviour
{
    private BattleManager bm;
    
    //Action Menu Variables
    [SerializeField] private GameObject actionMenuButtonsContainer, selectedAction;
    public Color selectionColor, selectedColor;
    private bool isInActionMenu;
    private bool isControllerActive;

    //Target Menu Variables
    private Character selectedTarget;
    private GameObject selectedTargetArrow {
        get {
            if (selectedTarget != null) {
                return selectedTarget.gameObject.transform.GetChild(0).gameObject;
            }
            return null;
        }
    }

    public void StartController() {
        bm = gameObject.GetComponent<BattleManager>();
    }
    public void StopController() {
        isControllerActive = false;
    }
    public void RestartController() {
        StartCoroutine(ChooseNewSelectedAction());
        isControllerActive = true;
        isInActionMenu = true;
    }
    private IEnumerator ChooseNewSelectedAction() {
        yield return null;
        
        if (actionMenuButtonsContainer.transform.GetChild(0).childCount > 0) {
            selectedAction = actionMenuButtonsContainer.transform.GetChild(0).GetChild(0).gameObject;
        } else {
            ActionMenuButton validButtonFound = null;
            for (int i = 0; i < actionMenuButtonsContainer.transform.childCount; i++) {
                if (validButtonFound == null) {
                    actionMenuButtonsContainer.transform.GetChild(i).gameObject.TryGetComponent<ActionMenuButton>(out validButtonFound);
                }
            }

            if (validButtonFound == null) {
                throw new System.Exception("No valid starting button found");
            }
            selectedAction = validButtonFound.gameObject;
        }
        selectedAction.GetComponent<Image>().color = selectionColor;
        gameObject.GetComponent<BattleUIManager>().UpdateTab(selectedAction);
    }
    
    private void OnNavigate(InputValue value) {
        if (!isControllerActive) {
            return;
        }

        Vector2 input = value.Get<Vector2>();

        if (isInActionMenu) {
            ActionMenuControls(input);
            gameObject.GetComponent<BattleUIManager>().UpdateTab(selectedAction);
        } else {
            TargetMenuControls(input);
        }
    }
    private void ActionMenuControls(Vector2 input) {
        selectedAction.GetComponent<Image>().color = new Color(1,1,1);

        if (input.x == 0) {
            if (input.y == 1) { //up
                selectedAction = selectedAction.GetComponent<ActionMenuButton>().prev;
            } else if (input.y == -1) { //down
                selectedAction = selectedAction.GetComponent<ActionMenuButton>().next;
            }
        } else {
            if (input.x == 1) {
                if (selectedAction.GetComponent<ActionMenuButton>().right != null) {
                    selectedAction = selectedAction.GetComponent<ActionMenuButton>().right;
                }
            } else if (input.x == -1) {
                if (selectedAction.GetComponent<ActionMenuButton>().left != null) {
                    selectedAction = selectedAction.GetComponent<ActionMenuButton>().left;
                }
            }
        }

        selectedAction.GetComponent<Image>().color = selectionColor;
    }
    private void TargetMenuControls(Vector2 input) {
        selectedTargetArrow.SetActive(false);

        if (input.x == 0) {
            if (input.y == 1) { //up
                selectedTarget = selectedTarget.prev;
            } else if (input.y == -1) { //down
                selectedTarget = selectedTarget.next;
            }
        } else {
            if (input.x == 1) { //right
                selectedTarget = selectedTarget.prev;
            } else if (input.x == -1) { //left
                selectedTarget = selectedTarget.next;
            }
        }

        selectedTargetArrow.SetActive(true);
    }

    private void OnSubmit() {
        BattleAction currAction = selectedAction.GetComponent<ActionMenuButton>().associatedAction;

        if (!isControllerActive) {
            return;
        }

        if (isInActionMenu) {
            //get eligible targets for action code
            List<Character> targets = bm.GetActionTargets(currAction);

            //do actionCode if it does not require a target, else go to target selection mode
            if (!currAction.needsTarget && targets.Count == 0) {
                bm.DoAction(currAction); 
            } else if (currAction.needsTarget && targets.Count > 0 ) {
                selectedAction.GetComponent<Image>().color = selectedColor;
                selectedTarget = targets[0];
                selectedTargetArrow.SetActive(true);
                
                isInActionMenu = false;
            } else {
                throw new Exception("Something funky when OnSubmit()");
            }
        } else {
            if (selectedAction.GetComponent<ActionMenuButton>().associatedWeapon != null) {
                bm.DoAction(currAction, selectedTarget, selectedAction.GetComponent<ActionMenuButton>().associatedWeapon);
            } else {
                bm.DoAction(currAction, selectedTarget);
            }

            //go back to action menu mode
            selectedAction.GetComponent<Image>().color = selectionColor;
            selectedTargetArrow.SetActive(false);
            selectedTarget = null;

            isInActionMenu = true;
        }
    }

    private void OnCancel() {
        if (!isControllerActive) {
            return;
        }

        if (!isInActionMenu) {
            //go back to action menu mode
            selectedTargetArrow.SetActive(false);
            selectedAction.GetComponent<Image>().color = selectionColor;
            isInActionMenu = true;
        }
    }

    private void OnTab() {
        if (!isControllerActive) {
            return;
        }

        gameObject.GetComponent<BattleUIManager>().ToggleTab(selectedAction);
    }
}