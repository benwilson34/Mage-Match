using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Tile {

	public enum Element {None, Fire, Water, Earth, Air, Muscle};
	public List<Element> elements;
	public int col = 0, row = 0;

    //private BitArray _elems; // 00000 : FWEAM

	public Tile(params Element[] elems){
        SetElements(elems);
	}

    public Tile(List<Element> elems) : this(elems.ToArray()) { }

	public Tile(char c) : this(CharToElement(c)) { }

    public void SetElements(params Element[] elems) {
        elements = new List<Element>();
        foreach (Element elem in elems) {
            elements.Add(elem);
        }
    }

    //public Element[] GetElements() {
        //List<Element> elems = new List<Element>();
        //for (int i = 0; i < _elems.Length; i++) {
        //    if (_elems[i])
        //        elems.Add((Element)(i + 1));
        //}
        //return elems.ToArray();
    //}

    public bool IsElement(Element elem) {
        //return _elems[ElemInd(elem)];
        foreach (Element tileElem in elements) {
            if (tileElem == elem)
                return true;
        }
        return false;
    }

    //int ElemInd(Element elem) {
    //    return (int)elem - 1;
    //}

	public void SetPos(int col, int row){
//		Debug.Log (color + " tile position set to (" + col + ", " + row + ")");
		this.col = col;
		this.row = row;
	}

	public bool HasSamePos(Tile comp){
		return this.col == comp.col && this.row == comp.row;
	}

    public Tile Copy() {
        Tile newT = new Tile(elements);
        newT.SetPos(col, row);
        return newT;
    }

    public string PrintCoord() { return "(" + col + "," + row + ")"; }

    //public string ElemsToString() {
    //    List<string> strs = new List<string>();
    //    for (int i = 0; i < _elems.Length; i++) {
    //        if (_elems[i])
    //            strs.Add(((Element)(i + 1)).ToString());
    //    }
    //    return string.Join(", ", strs.ToArray());
    //}


    public string ElementsToString(bool shortFormat = true) {
        if (elements.Count == 1)
            return shortFormat ? "" + ElementToChar(elements[0]) : elements[0].ToString();
        else {
            if (shortFormat) {
                string str = "";
                foreach (Element elem in elements)
                    str += ElementToChar(elem);
                return "(" + str + ")";
            } else {
                List<string> names = new List<string>();
                foreach (Element elem in elements)
                    names.Add(elem.ToString());
                return string.Join(", ", names.ToArray());
            }
        }
	}

    public static char ElementToChar(Element elem) {
        if (elem == Element.None)
                return '*';
        else return elem.ToString().Substring(0, 1)[0];
    }

	public static Element CharToElement(char c){
		c = char.ToUpper (c); //?
		switch (c) {
		case 'F':
			return Element.Fire;
		case 'W':
			return Element.Water;
		case 'E':
			return Element.Earth;
		case 'A':
			return Element.Air;
		case 'M':
			return Element.Muscle;
		default:
			return Element.None;
		}
	}
}
