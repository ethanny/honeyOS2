using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;


// Define the FileManager class that extends the App class
public class FileManager : App
{   

    // [SerializeField] private TMP_Text _currentAddress;
    // private Node root;
    // private Node currentDirectory;

    // public FileManager (){
    //     this.root = new Node (new FileDescriptor (">Home", "Home"));
    //     currentDirectory = root;
    // }

    // // public void Start (){
    // //     UpdateAddress();
    // // }

    // public void UpdateAddress(){
    //     if (currentDirectory != null && currentDirectory.FileDescriptor != null){
    //         string path = currentDirectory.FileDescriptor.Path;
    //         _currentAddress.text = path;
    //     }
    //     else{
    //         Debug.Log ("Current directory of its file descriptor is null.");
    //     }
    // }
}

// public class FileDescriptor
// {
//     public string Path { get; set; }
//     public string Contents { get; set; }
//     public string Name { get; set; }

//     public FileDescriptor(string path, string name)
//     {
//         Path = path;
//         Contents = "";
//         Name = name;
//     }
// }

// public class Node
// {
//     public FileDescriptor FileDescriptor { get; }
//     public List<Node> Children { get; }
//     public Node Parent { get; private set; }

//     public Node(FileDescriptor fileDescriptor)
//     {
//         FileDescriptor = fileDescriptor;
//         Children = new List<Node>();
//     }

//     public void SetParent(Node parent)
//     {
//         Parent = parent;
//     }

//     public void AddChild(Node child)
//     {
//         child.SetParent(this);
//         Children.Add(child);
//     }

//     public void RemoveChild(Node child){
//         Children.Remove(child);
//     }
// }