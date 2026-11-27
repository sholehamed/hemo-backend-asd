namespace Application.Common.Models;

using System;
using System.Collections.Generic;

public record TreeNode<T, Source> where T : class
{
   public bool? Checked { get; set; }
    public string Label { get; set; }
    public T Data { get; set; }
  //  public string Icon { get; set; }
    //public string ExpandedIcon { get; set; }
   // public string CollapsedIcon { get; set; }
    public List<Source> Children { get; set; }
    public bool? Leaf { get; set; }
    public bool? Expanded { get; set; }
   // public string Type { get; set; }
  //  public Source Parent { get; set; }
 //   public bool? PartialSelected { get; set; }
 //   public object Style { get; set; }
//    public string StyleClass { get; set; }
 //   public bool? Draggable { get; set; }
 //   public bool? Droppable { get; set; }
    public bool? Selectable { get; set; }
    public Guid Key { get; set; }
 //   public bool? Loading { get; set; }
}

