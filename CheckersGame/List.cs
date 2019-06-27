using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersGame
{
    internal interface Enumeration
    {
        bool hasMoreElements();
        object nextElement();
    }
    // Позиция
    class ListNode
    {
        internal ListNode prev, next;
        internal object value;

        public ListNode(object elem, ListNode prevNode, ListNode nextNode)
        {
            value = elem;
            prev = prevNode;
            next = nextNode;
        }
    }

    // Список ходов
    class List
    {
        private ListNode head;           // Голова
        private ListNode tail;          // Хвост
        private int count;

        public List()
        {
            count = 0;
        }

        // Проверка списка
        public bool isEmpty()
        {
            return head == null;
        }

        // Добавление элемента в хвост списка
        public void push_back(object elem)
        {
            ListNode node = new ListNode(elem, tail, null);

            if (tail != null)
                tail.next = node;
            else
                head = node;

            tail = node;
            count++;
        }

        // Добавление элемента в голову списка
        public void push_front(object elem)
        {
            ListNode node = new ListNode(elem, null, head);

            if (head != null)
                head.prev = node;
            else
                tail = node;

            head = node;
            count++;
        }

        // Возврат элемента из головы списка
        public object peek_head()
        {
            if (head != null)
                return head.value;
            else
                return null;
        }

        // Возврат элемента из хвоста списка без его удаления
        public object peek_tail()
        {
            if (tail != null)
                return tail.value;
            else
                return null;
        }

        // Удаление элемента из хвоста списка
        public object pop_back()
        {
            if (tail == null)
                return null;

            ListNode node = tail;
            tail = tail.prev;

            if (tail != null)
                tail.next = null;
            else
                head = null;

            count--;
            return node.value;
        }

        // Проверка если клетка выбрана
        public bool has(object elem)
        {
            ListNode node = head;

            while (node != null && !node.value.Equals(elem))
                node = node.next;

            return node != null;
        }

        // Удаление всех элементов из списка
        public void clear()
        {
            head = tail = null;
        }

        // Соединение списков
        public void append(List other)
        {
            ListNode node = other.head;

            while (node != null)
            {
                push_back(node.value);
                node = node.next;
            }
        }

        public object clone()
        {
            List temp = new List();
            ListNode node = head;

            while (node != null)
            {
                temp.push_back(node.value);
                node = node.next;
            }
            return temp;
        }

        public object pop_front()
        {
            if (head == null)
                return null;

            ListNode node = head;
            head = head.next;

            if (head != null)
                head.prev = null;
            else
                tail = null;

            count--;
            return node.value;
        }
        internal class EnumMoove: Enumeration
        {
            private ListNode node;

            internal EnumMoove(ListNode start)
            {
                node = start;
            }
            public bool hasMoreElements()
            {
                return node != null;
            }
            public object nextElement()
            {
                object temp;
                temp = node.value;
                node = node.next;
                return temp;
            }
        }
        public Enumeration elements()
        {
            return new EnumMoove(head);
        }
    }
    class Move
    {
        private int from;

        private int to;
        public Move(int moveFrom, int moveTo)
        {
            from = moveFrom;
            to = moveTo;
        }
        public int getFrom()
        {
            return from;
        }
        public int getTo()
        {
            return to;
        }
    }
}
