﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AiMovementLogic {

    public AiBase aiBase;
    float characterColliderWidth;
    float characterColliderHeight;
    Collider2D collider;

    //logic
    public AiMapLogic aiMapLogic;

    protected Vector2 up = Vector2.up;
    protected Vector2 down = Vector2.down;
    protected Vector2 left = Vector2.left;
    protected Vector2 right = Vector2.right;
    protected Vector2 stop = Vector2.zero;

    public Vector2 charBotLeft;
    public Vector2 charBotRight;
    public Vector2 charTopLeft;
    public Vector2 charTopRight;

    public Vector2 currentTargetDestination;

    //detect only collision with barriers and borders (assigned manually to prefab)
    public LayerMask barrierMask;
    public LayerMask notproofBarrierMask;

    //public AiAvoidBulletLogic aiAvoidBulletLogic;

    public AiMovementLogic(AiBase aiBase)
    {
        this.aiBase = aiBase;

        characterColliderWidth = aiBase.characterColliderWidth;
        characterColliderHeight = aiBase.characterColliderHeight;
        collider = aiBase.GetComponent<Collider2D>();

        walkingFront = new List<Vector2>();
        barrierMask = aiBase.barrierMask;

        notproofBarrierMask = 1 << 14;
    }
    
    public List<Vector2> walkingFront;

    public List<Vector2> GetNodes()
    {
        List<Vector2> nodes = new List<Vector2>();
        nodes.Add(new Vector2(0, 0));
        nodes.Add(new Vector2(1, 0));
        nodes.Add(new Vector2(1, 1));
        nodes.Add(new Vector2(2, 1));
        return nodes;
    }

    /// <summary>
    /// node for pathfinding algorithm
    /// </summary>
    public class PathNode
    {
        public Vector2 node;
        public int parentIndex;

        public PathNode(Vector2 node, int parentIndex)
        {
            this.node = node;
            this.parentIndex = parentIndex;

        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            PathNode p = obj as PathNode;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return node == p.node;
        }

        public override string ToString()
        {
            return node.ToString();
        }
    }

    public List<Vector2> GetPathTo(Vector2 target)
    {
        float step = characterColliderWidth;

        List<Vector2> path = new List<Vector2>();
        PathNode startNode = new PathNode(new Vector2(aiBase.posX, aiBase.posY), 0);
        List<PathNode> visitedNodes = new List<PathNode>();

        bool found = false;
        int finalNodeIndex = 0;
        //trying 5 different starting points!!!!!!!!!!!!!!! - only 1 for now
        for (int start = 0; start < 1; start++)
        {
            visitedNodes.Clear();
            switch (start)
            {
                case 0://center
                    startNode = new PathNode(new Vector2(aiBase.posX, aiBase.posY), 0);
                    break;
                case 1://left
                    startNode = new PathNode(new Vector2(aiBase.posX - step / 2, aiBase.posY), 0);
                    break;
                case 2://up
                    startNode = new PathNode(new Vector2(aiBase.posX, aiBase.posY + step / 2), 0);
                    break;
                case 3://right
                    startNode = new PathNode(new Vector2(aiBase.posX + step / 2, aiBase.posY), 0);
                    break;
                case 4://down
                    startNode = new PathNode(new Vector2(aiBase.posX, aiBase.posY - step / 2), 0);
                    break;
                default:
                    break;
            }

            visitedNodes.Add(startNode);

            for (int i = 0; i < visitedNodes.Count; i++)
            {
                PathNode currentNode = visitedNodes[i];
                //end process when current node is close to target
                if (aiMapLogic.GetDistance(currentNode.node, target) < step)
                {
                    finalNodeIndex = i;
                    found = true;
                    break;
                }
                if (i > 3000)
                {
                    finalNodeIndex = 100;
                    break;
                }


                //set neighbouring nodes
                PathNode nodeLeft = new PathNode(new Vector2(currentNode.node.x - step, currentNode.node.y), i);
                PathNode nodeUp = new PathNode(new Vector2(currentNode.node.x, currentNode.node.y + step), i);
                PathNode nodeRight = new PathNode(new Vector2(currentNode.node.x + step, currentNode.node.y), i);
                PathNode nodeDown = new PathNode(new Vector2(currentNode.node.x, currentNode.node.y - step), i);
                //add to list if there is no collision and they are not already in list
                if (!CharacterCollidesBarrier(nodeLeft.node) && !visitedNodes.Contains(nodeLeft) &&!CharacterCollidesMine(nodeLeft.node))
                { visitedNodes.Add(nodeLeft); }
                if (!CharacterCollidesBarrier(nodeUp.node) && !visitedNodes.Contains(nodeUp) && !CharacterCollidesMine(nodeUp.node))
                { visitedNodes.Add(nodeUp); }
                if (!CharacterCollidesBarrier(nodeRight.node) && !visitedNodes.Contains(nodeRight) && !CharacterCollidesMine(nodeRight.node))
                { visitedNodes.Add(nodeRight); }
                if (!CharacterCollidesBarrier(nodeDown.node) && !visitedNodes.Contains(nodeDown) && !CharacterCollidesMine(nodeDown.node))
                { visitedNodes.Add(nodeDown); }
            }

            if (found)
                break;
        }

        //reversly find the way back to starting node
        int index = finalNodeIndex;
        while (index != 0)
        {
            path.Add(visitedNodes[index].node);
            index = visitedNodes[index].parentIndex;
        }
        //reverse path
        path.Reverse();
        return path;
    }

    public bool MoveTo(Vector2 position)
    {
        return MoveTo(position, 0.15f);
    }

    public bool MoveTo(Vector2 position, float e)
    {
        return MoveTo(position.x, position.y, e);
    }

    public bool MoveTo(float x, float y)
    {
        return MoveTo(x, y, 0.15f);
    }

    /// <summary>
    /// gives commands to unit in order to get to the given coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public bool MoveTo(float x, float y, float e)
    {
        //Debug.DrawRay(new Vector2(x, y), aiBase.up, Color.yellow, 0.2f);
        if (ValueEquals(aiBase.posX, x, e) && ValueEquals(aiBase.posY, y, e))
        {
            Stop();
            return true;
        }
        else if (CharacterCollidesMine(new Vector2(x, y)))
        {
            Stop();
        }

        //refresh path only when target moves or there is mine collision
        if (!ValueEquals(currentTargetDestination.x, x, aiBase.characterWidth) ||
            !ValueEquals(currentTargetDestination.y, y, characterColliderHeight) ||
            CharacterCollidesMine(new Vector2(x, y)))
        {
            walkingFront = GetPathTo(new Vector2(x, y));
            currentTargetDestination = new Vector2(x, y);
        }
        
        //walk the small distance
        if (walkingFront.Count == 0)
        {
            if (Time.frameCount % 30 < 15)
            {
                PreferX(x, y);
            }
            else
            {
                PreferY(x, y);
            }
        }
        else
        {
            //draw path - DEBUGGING
            /*
            for (int i = 0; i < walkingFront.Count; i++)
            {
                
                if (i + 1 != walkingFront.Count)
                    Debug.DrawLine(walkingFront[i], walkingFront[i + 1], Color.blue);
                
            }
            */

            Vector2 currentNode = walkingFront[0];
            if (ValueEquals(aiBase.gameObject.transform.position.y, currentNode.y) && ValueEquals(aiBase.gameObject.transform.position.x, currentNode.x))
            {
                walkingFront.RemoveAt(0);
            }
            else
            {
                if (Time.frameCount % 30 < 15)
                {
                    PreferX(currentNode.x, currentNode.y);
                }
                else
                {
                    PreferY(currentNode.x, currentNode.y);
                }
            }
        }
        return false;
    }

    public List<Vector2> GetAvailableSpotsAround(Vector2 center, float distance)
    {
        Vector2 up = new Vector2(center.x, center.y + distance);
        Vector2 right = new Vector2(center.x + distance, center.y );
        Vector2 down = new Vector2(center.x, center.y - distance);
        Vector2 left = new Vector2(center.x - distance, center.y);

        List<Vector2> availableSpots = new List<Vector2>();
        if (IsWithinBoundaries(up) && !CharacterCollidesBarrier(up))
            availableSpots.Add(up);
        if (IsWithinBoundaries(right) && !CharacterCollidesBarrier(right))
            availableSpots.Add(right);
        if (IsWithinBoundaries(down) && !CharacterCollidesBarrier(down))
            availableSpots.Add(down);
        if (IsWithinBoundaries(left) && !CharacterCollidesBarrier(left))
            availableSpots.Add(left);
        return availableSpots;
    }

    bool IsWithinBoundaries(Vector2 point)
    {
        return 
            point.x > aiBase.mapMinX &&
            point.x < aiBase.mapMaxX &&
            point.y > aiBase.mapMinY &&
            point.y < aiBase.mapMaxY;
    }
    
    /// <summary>
    /// chekovat pouze jeden směr nestačí
    /// </summary>
    /// <param name="center"></param>
    /// <returns></returns>
    public bool CharacterCollides(Vector2 center)
    {
        Vector2 colliderOffset = aiBase.GetComponent<Collider2D>().offset;
        float colliderWidth = aiBase.GetComponent<Collider2D>().bounds.size.x;
        float distance = 0.1f;

        bool colRight = ObjectCollides(center + colliderOffset, colliderWidth, right, distance);
        //bool colLeft = ObjectCollides(center + colliderOffset, colliderWidth, left, distance);
        bool colUp = ObjectCollides(center + colliderOffset / 2, colliderWidth, up, distance);
        //bool colDown = ObjectCollides(center + colliderOffset, colliderWidth, down, distance);

        //return colRight || colLeft || colUp || colDown;
        //return colRight || colLeft;
        //return colRight;
        return colUp;
    }
    
    public bool CharacterCollidesNotproofBarrier(Vector2 center)
    {
        float width = characterColliderWidth / 2;
        float height = characterColliderHeight / 2;
        Vector2 colliderOffset = aiBase.GetComponent<Collider2D>().offset / 2;
        float offset = 0.1f;

        Vector2 botLeft = new Vector2(center.x - width - offset, center.y - height - offset);
        Vector2 botRight = new Vector2(center.x + width + offset, center.y - height - offset);
        Vector2 topLeft = new Vector2(center.x - width - offset, center.y + height + offset);
        Vector2 topRight = new Vector2(center.x + width + offset, center.y + height + offset);



        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(botLeft, aiBase.direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(botRight, aiBase.direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(topLeft, aiBase.direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(topRight, aiBase.direction);


        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, aiBase.direction, 0.1f, notproofBarrierMask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, aiBase.direction, 0.1f, notproofBarrierMask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, aiBase.direction, 0.1f, notproofBarrierMask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, aiBase.direction, 0.1f, notproofBarrierMask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
        {
            return true;            
        }


        return false;
    }

    /// <summary>
    /// new collisioncheck using box collider bounds
    /// </summary>
    /// <returns></returns>
    public bool CharacterCollidesBarrier(Vector2 center)
    {
        float width = characterColliderWidth / 2;
        float height = characterColliderHeight / 2;
        float offset = 0.1f;

        Vector2 botRight = new Vector2(center.x + width + offset, center.y - height - offset);
        Vector2 topLeft = new Vector2(center.x - width - offset, center.y + height + offset);

        Collider2D[] collisions = Physics2D.OverlapAreaAll(topLeft, botRight, aiBase.barrierMask);
        if (collisions.Length != 0)
            return true;
        return false;
    }

    public bool CharacterCollidesMine(Vector2 center)
    {
        float width = characterColliderWidth / 2;
        float height = characterColliderHeight / 2;
        Vector2 colliderOffset = aiBase.GetComponent<Collider2D>().offset / 2;
        float offset = 0.1f;
        
        Vector2 botRight = new Vector2(center.x + width + offset, center.y - height - offset);
        Vector2 topLeft = new Vector2(center.x - width - offset, center.y + height + offset);

        Collider2D[] bulletsColliders = Physics2D.OverlapAreaAll(topLeft, botRight, aiBase.bulletMask);
        foreach(Collider2D c in bulletsColliders)
        {
            try
            {
                Bullet b = c.GetComponentInParent<Bullet>();
                if(b.owner.playerNumber == aiBase.playerNumber)
                {
                    return true;
                }
                else
                {

                }
            }
            catch (Exception e)
            {

            }
        }
        return false;
    }
    
    /// <summary>
    /// bot, top, left, right might be obsolete
    /// </summary>
    /// <returns></returns>
    public bool ObjectCollides(Vector2 center, float width, Vector2 direction, float distance)
    {
        width += 0.1f; //small offset
        LayerMask layerMask = barrierMask;
        charBotLeft = new Vector2(center.x - width / 2, center.y - width / 2);
        charBotRight = new Vector2(center.x + width / 2, center.y - width / 2);
        charTopLeft = new Vector2(center.x - width / 2, center.y + width / 2);
        charTopRight = new Vector2(center.x + width / 2, center.y + width / 2);
        
        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, aiBase.direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, aiBase.direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, aiBase.direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, aiBase.direction);
        

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, aiBase.direction, distance, layerMask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, aiBase.direction, distance, layerMask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, aiBase.direction, distance, layerMask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, aiBase.direction, distance, layerMask);
        
        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
        {
            return true;
        }

        return false;
    }

    public bool CollidesBarrierFrom(Vector2 point, Vector2 direction, float distance)
    {
        LayerMask layerMask = barrierMask;
        return Physics2D.Raycast(point, direction, distance, layerMask);
    }

    public bool CollidesBarrier(Vector2 direction, float distance)
    {
        LayerMask layerMask = barrierMask;

        charBotLeft = new Vector2(collider.bounds.min.x, collider.bounds.min.y);
        charBotRight = new Vector2(collider.bounds.max.x, collider.bounds.min.y);
        charTopLeft = new Vector2(collider.bounds.min.x, collider.bounds.max.y);
        charTopRight = new Vector2(collider.bounds.max.x, collider.bounds.max.y);

        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, aiBase.direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, aiBase.direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, aiBase.direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, aiBase.direction);


        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, direction, distance, layerMask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, direction, distance, layerMask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, direction, distance, layerMask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, direction, distance, layerMask);
        
        int hitCount = 0;
        if (hitBotLeft)
            hitCount++;
        if (hitBotRight)
            hitCount++;
        if (hitTopLeft)
            hitCount++;
        if (hitTopRight)
            hitCount++;
        
        if(hitCount > 0)
            return true;

        return false;
    }

    public bool CollidesBarrier(Vector2 direction)
    {
        return CollidesBarrier(direction, 0.1f);
    }

    public bool CollidesBarrier(float distance)
    {
        return CollidesBarrier(left, distance) || CollidesBarrier(up, distance) || CollidesBarrier(right, distance) || CollidesBarrier(down, distance);
    }
   
    //old
    public bool Collides(Vector2 center, float extens, float width, LayerMask layermask, Vector2 direction)
    {
        charBotLeft = new Vector2(center.x - extens + width, center.y - extens + width);
        charBotRight = new Vector2(center.x + extens - width, center.y - extens + width);
        charTopLeft = new Vector2(center.x - extens + width, center.y + extens - width);
        charTopRight = new Vector2(center.x + extens - width, center.y + extens - width);

        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, direction);


        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, aiBase.direction, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, aiBase.direction, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, aiBase.direction, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, aiBase.direction, width, layermask);
        

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        return false;
    }
    //old
    public bool Collides(Vector2 center, float extens, float width, LayerMask layermask)
    {
        charBotLeft = new Vector2(center.x - extens + width, center.y - extens + width);
        charBotRight = new Vector2(center.x + extens - width, center.y - extens + width);
        charTopLeft = new Vector2(center.x - extens + width, center.y + extens - width);
        charTopRight = new Vector2(center.x + extens - width, center.y + extens - width);

        RaycastHit2D hitBotLeft;
        Ray rayBotLeft = new Ray(charBotLeft, aiBase.direction);
        RaycastHit2D hitBotRight;
        Ray rayBotRight = new Ray(charBotRight, aiBase.direction);
        RaycastHit2D hitTopLeft;
        Ray rayTopLeft = new Ray(charTopLeft, aiBase.direction);
        RaycastHit2D hitTopRight;
        Ray rayTopRight = new Ray(charTopRight, aiBase.direction);

        Vector2 allDirection = new Vector2(0, 0);

        allDirection = left;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        allDirection = up;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        allDirection = right;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        allDirection = down;

        hitBotLeft = Physics2D.Raycast(rayBotLeft.origin, allDirection, width, layermask);
        hitBotRight = Physics2D.Raycast(rayBotRight.origin, allDirection, width, layermask);
        hitTopLeft = Physics2D.Raycast(rayTopLeft.origin, allDirection, width, layermask);
        hitTopRight = Physics2D.Raycast(rayTopRight.origin, allDirection, width, layermask);

        if (hitBotLeft || hitBotRight || hitTopLeft || hitTopRight)
            return true;

        return false;
    }
    //old
    public bool Collides(Vector2 center, Vector2 direction)
    {
        return Collides(center, 0.5f, 0.3f, barrierMask, direction);
    }
    
    public string PrintNodeList(List<Vector2> list)
    {
        string result = "";
        foreach (Vector2 node in list)
        {
            result += "[" + node.x + "," + node.y + "],";
        }
        return result;
    }
    
    /// <summary>
    /// moves to [x,y] preferably on x axis
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void PreferX(double x, double y)
    {
        if (ValueEquals(aiBase.gameObject.transform.position.y, y)) { }
        else if (ValueSmaller(aiBase.gameObject.transform.position.y, y)) { MoveUp(); }
        else { MoveDown(); }

        if (ValueEquals(aiBase.gameObject.transform.position.x, x)) { }
        else if (ValueSmaller(aiBase.gameObject.transform.position.x, x)) { MoveRight(); }
        else { MoveLeft(); }
    }
    /// <summary>
    /// moves  to [x,y] preferably on y axis
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void PreferY(double x, double y)
    {
        if (ValueEquals(aiBase.gameObject.transform.position.x, x)) { }
        else if (ValueSmaller(aiBase.gameObject.transform.position.x, x)) { MoveRight(); }
        else { MoveLeft(); }

        if (ValueEquals(aiBase.gameObject.transform.position.y, y)) { }
        else if (ValueSmaller(aiBase.gameObject.transform.position.y, y)) { MoveUp(); }
        else { MoveDown(); }
    }
    
    public void Move(Vector2 direction)
    {
        if (direction == left)
            MoveLeft();
        else if (direction == right)
            MoveRight();
        else if (direction == up)
            MoveUp();
        else if (direction == down)
            MoveDown();
    }
    public void MoveUp()
    {
        aiBase.rb2d.velocity = Vector2.up * aiBase.speed;
        aiBase.direction = up;
        aiBase.UpdateAnimatorState(AnimatorStateEnum.walkUp);
    }
    public void MoveLeft()
    {
        aiBase.rb2d.velocity = Vector2.left * aiBase.speed;
        aiBase.direction = left;
        aiBase.UpdateAnimatorState(AnimatorStateEnum.walkLeft);
    }
    public void MoveDown()
    {
        aiBase.rb2d.velocity = Vector2.down * aiBase.speed;
        aiBase.direction = down;
        aiBase.UpdateAnimatorState(AnimatorStateEnum.walkDown);
    }
    public void MoveRight()
    {
        aiBase.rb2d.velocity = Vector2.right * aiBase.speed;
        aiBase.direction = right;
        aiBase.UpdateAnimatorState(AnimatorStateEnum.walkRight);
    }
    public void Stop()
    {
        aiBase.rb2d.velocity = Vector2.zero;
        //aiBase.UpdateAnimatorState(AnimatorStateEnum.stop);
    }

    private float e = 0.1f; //odchylka
    /// <summary>
    /// porovnání z důvodu odchylky způsobené pohybem (škubáním)
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public bool ValueSmaller(double value1, double value2)
    {
        if (value1 - e < value2 || value1 + e < value2)
            return true;
        else
            return false;
    }

    public bool ValueEquals(double value1, double value2)
    {
        return ValueEquals(value1, value2, e);
    }

    public bool ValueEquals(double value1, double value2, float e)
    {
        if (value2 - e <= value1 && value1 <= value2 + e)
            return true;
        else
            return false;
    }

}
