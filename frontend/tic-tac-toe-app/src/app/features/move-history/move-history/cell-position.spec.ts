import { formatCellPosition } from './cell-position';

describe('formatCellPosition', () => {
  it('maps index 0 to row 1, column 1', () => {
    expect(formatCellPosition(0)).toBe('Row 1, Column 1');
  });

  it('maps index 4 (center) to row 2, column 2', () => {
    expect(formatCellPosition(4)).toBe('Row 2, Column 2');
  });

  it('maps index 8 (bottom-right) to row 3, column 3', () => {
    expect(formatCellPosition(8)).toBe('Row 3, Column 3');
  });

  it('maps index 5 to row 2, column 3', () => {
    expect(formatCellPosition(5)).toBe('Row 2, Column 3');
  });
});
