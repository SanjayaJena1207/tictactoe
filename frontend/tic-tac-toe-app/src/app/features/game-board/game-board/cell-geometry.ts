/** Pure presentation helpers for positioning the winning-line SVG overlay.
 *  No game logic — only maps a board index to its center point, expressed
 *  as a percentage of the board's own box, so the overlay scales with it. */

export interface Point {
  x: number;
  y: number;
}

export function cellCenter(index: number): Point {
  const row = Math.floor(index / 3);
  const col = index % 3;
  return { x: (col + 0.5) * (100 / 3), y: (row + 0.5) * (100 / 3) };
}

export interface LineEndpoints {
  from: number;
  to: number;
}

/** The three winning indices are always collinear and evenly spaced, so the
 *  min and max index are the line's endpoints regardless of array order. */
export function lineEndpoints(cells: number[] | null | undefined): LineEndpoints | null {
  if (!cells || cells.length < 2) {
    return null;
  }
  const sorted = [...cells].sort((a, b) => a - b);
  return { from: sorted[0], to: sorted[sorted.length - 1] };
}
