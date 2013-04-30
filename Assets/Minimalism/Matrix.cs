using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Matrix<T> {
	
	private int Height;
	private int Width;
	
	private T[] values;
	
	
	public Matrix(int width, int height) {
		this.Height = height;
		this.Width = width;
		values = new T[width*height];
	}
	
	public T GetValueAt(int col, int row) {
		return values[(row*Height)+col];
	}
	
	public T GetValueAt(Vector2 position) {
		return values[(((int)position.y)*Height)+((int)position.x)];
	}
	
	public void SetValueAt(int col, int row, T val) {
		values[(row*Height)+col] = val;
	}
	
	public bool getPosition(T val, out Vector2 position) {
		for (int row=0; row < values.Length; row++) {
			for (int col=0; col < values.Length; col++) {
				T currVal = GetValueAt(col, row);
				if (null != currVal && currVal.Equals(val)) {
					position = new Vector2(col, row);
					return true;
				}
			}
		}
		position = new Vector2(-1.0f, -1.0f);
		return false;
	}
	
	public List<T> getValues() {
		List<T> vals = new List<T>();
		foreach (T v in values) {
			if (null != v) {
				vals.Add(v);
			}
		}
		return vals;
	}
	
	public T[] GetRow(int rowIndex) {
		T[] row = new T[Width];
		for (int col=0; col < Width; col++) {
			row[col] = GetValueAt(col, rowIndex);
		}
		return row;
	}
	
	public T[] GetColumn(int colIndex) {
		T[] col = new T[Height];
		for (int row=0; row < Height; row++) {
			col[row] = GetValueAt(colIndex, row);
		}
		return col;
	}
	
	public void SlideRowBy(int rowIndex, int values) {
		T[] oldRow = GetRow(rowIndex);
		for (int col=0; col < Width; col++) {
			int oldPosition = col - values;
			while (oldPosition < 0) {
				oldPosition += Width;
			}
			while (oldPosition >= Width) {
				oldPosition -= Width;
			}
			SetValueAt(col, rowIndex, oldRow[oldPosition]);
		}
	}
	
	// positive values slide up, negative slide down
	public void SlideColumnBy(int colIndex, int values) {
		T[] oldColumn = GetColumn(colIndex);
		for (int row=0; row < Height; row++) {
			int oldPosition = row - values;
			while (oldPosition < 0) {
				oldPosition += Height;
			}
			while (oldPosition >= Height) {
				oldPosition -= Height;
			}
			SetValueAt(colIndex, row, oldColumn[oldPosition]);
		}
	}
	
}
