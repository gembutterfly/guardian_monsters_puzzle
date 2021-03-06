﻿using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Board Variables")]
    public float swipeAngle = 0;
    public int col;
    public int row;
    public int previousCol;
    public int previousRow;
    public int targetX;
    public int targetY;
    public float swipeResist = 1f; // do not swipe if stroke is shorter

    public bool isMatched = false;


    private Vector2 initialTouchPosition;
    private Vector2 finalTouchPosition;

    private Vector2 tempPosition;

    private Board board;
    private GameObject otherBlock;


	// Use this for initialization
	void Start ()
    {
        board = FindObjectOfType<Board>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //col = targetX;
        //row = targetY;
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        FindMatches();

        if(isMatched)
        {
            var mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(.5f, .5f, .5f, .5f);
        }

        targetX = col;
        targetY = row;

        if(Mathf.Abs(targetX - transform.position.x) > .1f)
        {
            // Move block to target position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if(board.allBlocks[col, row] != this.gameObject)
            {
                board.allBlocks[col, row] = this.gameObject;
            }
        } else
        {
            // Set to final position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            board.allBlocks[col, row] = this.gameObject;
        }

        if(Mathf.Abs(targetY - transform.position.y) > .1f)
        {
            // Move block to target position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if(board.allBlocks[col, row] != this.gameObject)
            {
                board.allBlocks[col, row] = this.gameObject;
            }
        } else
        {
            // Set to final position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    // Co-Routine method
    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);

        if(otherBlock != null)
        {
            if(!isMatched && !otherBlock.GetComponent<Block>().isMatched)
            {
                // Undo last move
                otherBlock.GetComponent<Block>().row = row;
                otherBlock.GetComponent<Block>().col = col;
                col = previousCol;
                row = previousRow;
                yield return new WaitForSeconds(.5f);
                board.currentState = GameState.MOVE;
            } else
            {
                board.DestroyMatches();
            }
            otherBlock = null;
        }
    }

    private void OnMouseDown()
    {
        if(board.currentState == GameState.MOVE)
        {
            // Convert position to Unity units
            initialTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if(board.currentState == GameState.MOVE)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }


    void CalculateAngle()
    {
        if(Vector2.Distance(initialTouchPosition, finalTouchPosition) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(
                finalTouchPosition.y - initialTouchPosition.y,
                finalTouchPosition.x - initialTouchPosition.x
                ) * 180 / Mathf.PI;
            MoveBlocks();
            board.currentState = GameState.WAIT;
        } else
        {
            board.currentState = GameState.MOVE;
        }
    }

    void MoveBlocks()
    {
        if(swipeAngle > -45 && swipeAngle <= 45 && col < board.width - 1)
        {
            // Right Swipe
            otherBlock = board.allBlocks[col + 1, row];
            previousCol = col;
            previousRow = row;
            otherBlock.GetComponent<Block>().col -= 1;
            col += 1;
        } else if(swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            // Up Swipe
            otherBlock = board.allBlocks[col, row + 1];
            previousCol = col;
            previousRow = row;
            otherBlock.GetComponent<Block>().row -= 1;
            row += 1;
        } else if(swipeAngle > 135 || swipeAngle <= -135 && col > 0)
        {
            // Left Swipe
            otherBlock = board.allBlocks[col - 1, row];
            previousCol = col;
            previousRow = row;
            otherBlock.GetComponent<Block>().col += 1;
            col -= 1;
        } else if(swipeAngle <= -45 && swipeAngle > -135 && row > 0)
        {
            // Down Swipe
            otherBlock = board.allBlocks[col, row - 1];
            previousCol = col;
            previousRow = row;
            otherBlock.GetComponent<Block>().row += 1;
            row -= 1;
        }

        // Call Coroutine
        StartCoroutine(CheckMoveCo());
    }

    void FindMatches()
    {
        if(col > 0 && col < board.width -1)
        {
            var leftBlock1 = board.allBlocks[col - 1, row];
            var rightBlock1 = board.allBlocks[col + 1, row];

            if(leftBlock1 != null && rightBlock1 != null)
            {
                if(leftBlock1.tag == this.gameObject.tag && rightBlock1.tag == this.gameObject.tag)
                {
                    // All three match
                    leftBlock1.GetComponent<Block>().isMatched = true;
                    rightBlock1.GetComponent<Block>().isMatched = true;
                    this.isMatched = true;
                }
            }
        }

        if(row > 0 && row < board.height - 1)
        {
            var upperBlock1 = board.allBlocks[col, row + 1];
            var lowerBlock1 = board.allBlocks[col, row - 1];

            if(upperBlock1 != null && lowerBlock1 != null)
            {
                if(upperBlock1.tag == this.gameObject.tag && lowerBlock1.tag == this.gameObject.tag)
                {
                    // All three match
                    upperBlock1.GetComponent<Block>().isMatched = true;
                    lowerBlock1.GetComponent<Block>().isMatched = true;
                    this.isMatched = true;
                }
            }
        }
    }
}
