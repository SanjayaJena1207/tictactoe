/** Converts a 0-indexed board cell (0-8) to a 1-indexed "Row X, Column Y" label. */
export function formatCellPosition(cellIndex: number): string {
  const row = Math.floor(cellIndex / 3) + 1;
  const column = (cellIndex % 3) + 1;
  return `Row ${row}, Column ${column}`;
}
