using UnityEngine;
using System.Collections;

public class Tile {

//	public char color;
	public int col = 0, row = 0;
	public enum Element {None, Fire, Water, Earth, Air, Muscle};
	public Element element;

	public Tile(Element element){
		this.element = element;
	}

	public Tile(char c){
		this.element = CharToElement(c);
	}

	public void SetPos(int col, int row){
//		Debug.Log (color + " tile position set to (" + col + ", " + row + ")");
		this.col = col;
		this.row = row;
	}

	public bool HasSamePos(Tile comp){
		return this.col == comp.col && this.row == comp.row;
	}

	public static char ElementToChar(Element element){ // maybe not necessary?
		switch (element) {
		case Element.Fire:
			return 'F';
		case Element.Water:
			return 'W';
		case Element.Earth:
			return 'E';
		case Element.Air:
			return 'A';
		case Element.Muscle:
			return 'M';
		default:
			return '*';
		}
	}

	public char ThisElementToChar(){
        return ElementToChar(element);
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
