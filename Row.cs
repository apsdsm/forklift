﻿using System.Collections.Generic;

namespace ForkLift {

    /// <summary>
    /// A generic validation node. These are used to store validated versions of row data.
    /// </summary>
    public class Row {

        public int lineNumber = 0;
        public bool valid = true;
        public string errorMessage = "";

        // store the field data for this node
        public Dictionary<string, Field> fields = new Dictionary<string, Field>();

        // override the bracket accessor to make working with nodes a little more pleasant
        public Field this[string fieldName] {
            get {
                return fields[fieldName];
            }

            set {
                fields[fieldName] = value;
            }
        }

        /// <summary>
        /// Returns true if the node is empty.
        /// </summary>
        /// <param name="fieldName">field to query</param>
        /// <returns>true if field is empty</returns>
        public bool IsEmpty(string fieldName) {
            return fields[fieldName].value == "";
        }

        /// <summary>
        /// An easy way to flag a node as invalid, and set an error message at the same time. Will include the line number in the error message.
        /// </summary>
        /// <param name="node">the node to mark as invalid</param>
        /// <param name="message">the message to be appended to the node</param>
        public void SetErrorMessage(string message) {
            valid = false;
            errorMessage = "error (line " + lineNumber + ") : " + message;
        }
    }
}