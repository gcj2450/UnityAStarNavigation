using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Priority_Queue
{
    public class Priority_Queue_Example : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        //Ê¾Àý³ÌÐò
        public static void RunExample()
        {
            //First, we create the priority queue.
            //By default, priority-values are of type 'float'
            SimplePriorityQueue<string> priorityQueue = new SimplePriorityQueue<string>();

            //Now, let's add them all to the queue (in some arbitrary order)!
            priorityQueue.Enqueue("4 - Joseph", 4);
            priorityQueue.Enqueue("2 - Tyler", 0); //Note: Priority = 0 right now!
            priorityQueue.Enqueue("1 - Jason", 1);
            priorityQueue.Enqueue("4 - Ryan", 4);
            priorityQueue.Enqueue("3 - Valerie", 3);

            //Change one of the string's priority to 2.  
            //Since this string is already in the priority queue, we call UpdatePriority() to do this
            priorityQueue.UpdatePriority("2 - Tyler", 2);

            //Finally, we'll dequeue all the strings and print them out
            while (priorityQueue.Count != 0)
            {
                string nextUser = priorityQueue.Dequeue();
                Debug.Log(nextUser);
            }
            //Output:
            //1 - Jason
            //2 - Tyler
            //3 - Valerie
            //4 - Joseph
            //4 - Ryan

            //Notice that when two strings with the same priority were enqueued 
            //they were dequeued in the same order that they were enqueued.
        }
    }

    //FastPriorityQueue Ê¾Àý
    public static class FastPriorityQueueExample
    {
        //The class to be enqueued.
        public class User : FastPriorityQueueNode
        {
            public string Name { get; private set; }
            public User(string name)
            {
                Name = name;
            }
        }

        private const int MAX_USERS_IN_QUEUE = 10;

        public static void RunExample()
        {
            //First, we create the priority queue.  We'll specify a max of 10 users in the queue
            FastPriorityQueue<User> priorityQueue = new FastPriorityQueue<User>(MAX_USERS_IN_QUEUE);

            //Next, we'll create 5 users to enqueue
            User user1 = new User("1 - Jason");
            User user2 = new User("2 - Tyler");
            User user3 = new User("3 - Valerie");
            User user4 = new User("4 - Joseph");
            User user42 = new User("4 - Ryan");

            //Now, let's add them all to the queue (in some arbitrary order)!
            priorityQueue.Enqueue(user4, 4);
            priorityQueue.Enqueue(user2, 0); //Note: Priority = 0 right now!
            priorityQueue.Enqueue(user1, 1);
            priorityQueue.Enqueue(user42, 4);
            priorityQueue.Enqueue(user3, 3);

            //Change user2's priority to 2.  
            //Since user2 is already in the priority queue, we call UpdatePriority() to do this
            priorityQueue.UpdatePriority(user2, 2);

            //Finally, we'll dequeue all the users and print out their names
            while (priorityQueue.Count != 0)
            {
                User nextUser = priorityQueue.Dequeue();
                Console.WriteLine(nextUser.Name);
            }

            //Output:
            //1 - Jason
            //2 - Tyler
            //3 - Valerie
            //4 - Joseph
            //4 - Ryan

            //Notice that when two users with the same priority were enqueued
            //they were dequeued in the same order that they were enqueued.
        }
    }

}