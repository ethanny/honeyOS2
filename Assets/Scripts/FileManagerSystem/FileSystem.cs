using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;
using UnityEngine.UI;


// Define the FileManager class that extends the App class
public class FileSystem : MonoBehaviour
{   

    [SerializeField] private TMP_Text _currentAddress;
    [SerializeField] private TMP_Text _directoryName;
    public Node root;
    public Node currentDirectory;

    private const string saveFilePath = "filesystem.json";

    private void Awake(){
        LoadFileSystem();
        UpdateAddress();
    }

    private void OnDestroy(){
        SaveFileSystem();
    }

    private void LoadFileSystem(){
        if (File.Exists(saveFilePath)){
            string json = File.ReadAllText(saveFilePath);
            root = JsonUtility.FromJson<Node>(json);
        }
        else{
            root = new Node (new FileDescriptor ("File Location", "Root"));
            AddDefaultChildrenToRoot();
        }
        currentDirectory = root;
    }

    private void AddDefaultChildrenToRoot(){
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Home", "Home")));
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Gallery", "Gallery")));
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Desktop", "Desktop")));
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Downloads", "Downloads")));
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Documents", "Documents")));
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Pictures", "Pictures")));
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Hard Drive", "Hard Drive")));
        root.AddChild(new Node(new FileDescriptor(currentDirectory.FileDescriptor.Path + "/Network", "Network")));
    }

    private void SaveFileSystem(){
        string json = JsonUtility.ToJson(root);
        File.WriteAllText(saveFilePath, json);
        // Debug.Log("File system saved to: " + saveFilePath);
    }

    public FileSystem (){
        this.root = new Node (new FileDescriptor ("File Location", "Root"));
        currentDirectory = root;
    }

    public void UpdateAddress(){
        if (currentDirectory != null && currentDirectory.FileDescriptor != null){
            string path = currentDirectory.FileDescriptor.Path;
            _currentAddress.text = path;
        }
        else{
            // Debug.Log ("Current directory of its file descriptor is null.");
        }
    }

    public void ChangeDirectory (){
        string directoryName = _directoryName.text;
        Node targetDirectory = FindDirectory (root,directoryName);

        if (targetDirectory != null){
            currentDirectory = targetDirectory;
            UpdateAddress();
        }
        else{
            // Debug.Log("Directory " + directoryName + " not found");
        }
    }

    public Node FindDirectory(Node currentNode, string directoryName){
        if (currentNode.FileDescriptor.Name == directoryName){
            return currentNode;
        }

        foreach (Node child in currentNode.Children){
            Node foundNode = FindDirectory(child, directoryName);
            if(foundNode != null){
                return foundNode;
            }
        }
        return null;
    }
}

public class FileDescriptor
{
    public string Path { get; set; }
    public string Contents { get; set; }
    public string Name { get; set; }

    public FileDescriptor(string path, string name)
    {
        Path = path;
        Contents = "";
        Name = name;
    }
}

public class Node
{
    public FileDescriptor FileDescriptor { get; }
    public List<Node> Children { get; }
    public Node Parent { get; private set; }

    public Node(FileDescriptor fileDescriptor)
    {
        FileDescriptor = fileDescriptor;
        Children = new List<Node>();
    }

    public void SetParent(Node parent)
    {
        Parent = parent;
    }

    public void AddChild(Node child)
    {
        child.SetParent(this);
        Children.Add(child);
    }

    public void RemoveChild(Node child){
        Children.Remove(child);
    }
}