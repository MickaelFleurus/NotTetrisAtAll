// using UnityEngine;
// using UnityEngine.UIElements;

// [UxmlElement]
// public partial class ModelSelector : VisualElement
// {
//     private Label mLabel;
//     private Button mLeftArrow;
//     private Button mRightArrow;
//     private VisualElement mModelContainer;
//     private Label mModelLabel;

//     [UxmlAttribute]
//     public string labelText
//     {
//         get => mLabel?.text ?? "";
//         set
//         {
//             if (mLabel != null)
//                 mLabel.text = value;
//         }
//     }

//     [UxmlAttribute]
//     public string modelText
//     {
//         get => mModelLabel?.text ?? "";
//         set
//         {
//             if (mModelLabel != null)
//                 mModelLabel.text = value;
//         }
//     }

//     public ModelSelector()
//     {
//         style.flexDirection = FlexDirection.Column;
//         style.alignItems = Align.Center;
//         style.gap = 15;
//         style.paddingTop = 10;
//         style.paddingBottom = 10;

//         // Create label
//         mLabel = new Label("Label");
//         mLabel.style.fontSize = 32;
//         mLabel.style.color = Color.white;
//         mLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
//         Add(mLabel);

//         // Create row container for arrows and model
//         var rowContainer = new VisualElement();
//         rowContainer.style.flexDirection = FlexDirection.Row;
//         rowContainer.style.alignItems = Align.Center;
//         rowContainer.style.gap = 15;
//         rowContainer.style.width = Length.Percent(100);

//         // Left arrow button
//         mLeftArrow = new Button();
//         mLeftArrow.text = "◀";
//         mLeftArrow.style.width = 60;
//         mLeftArrow.style.height = 60;
//         mLeftArrow.style.fontSize = 24;
//         mLeftArrow.style.backgroundColor = new Color(1, 1, 1, 0.9f);
//         mLeftArrow.style.color = Color.black;
//         mLeftArrow.style.flexShrink = 0;
//         rowContainer.Add(mLeftArrow);

//         // Model container
//         mModelContainer = new VisualElement();
//         mModelContainer.style.flex = 1;
//         mModelContainer.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
//         mModelContainer.style.borderBottomLeftRadius = 25;
//         mModelContainer.style.borderBottomRightRadius = 25;
//         mModelContainer.style.borderTopLeftRadius = 25;
//         mModelContainer.style.borderTopRightRadius = 25;
//         mModelContainer.style.justifyContent = Justify.Center;
//         mModelContainer.style.alignItems = Align.Center;
//         mModelContainer.style.minHeight = 90;
//         mModelContainer.style.paddingLeft = 20;
//         mModelContainer.style.paddingRight = 20;

//         mModelLabel = new Label("Model!");
//         mModelLabel.style.fontSize = 28;
//         mModelLabel.style.color = Color.white;
//         mModelLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
//         mModelLabel.style.whiteSpace = WhiteSpace.NoWrap;
//         mModelContainer.Add(mModelLabel);

//         rowContainer.Add(mModelContainer);

//         // Right arrow button
//         mRightArrow = new Button();
//         mRightArrow.text = "▶";
//         mRightArrow.style.width = 60;
//         mRightArrow.style.height = 60;
//         mRightArrow.style.fontSize = 24;
//         mRightArrow.style.backgroundColor = new Color(1, 1, 1, 0.9f);
//         mRightArrow.style.color = Color.black;
//         mRightArrow.style.flexShrink = 0;
//         rowContainer.Add(mRightArrow);

//         Add(rowContainer);
//     }

//     public void SetOnLeftArrowClicked(System.Action callback) => mLeftArrow.clicked += callback;
//     public void SetOnRightArrowClicked(System.Action callback) => mRightArrow.clicked += callback;
// }
