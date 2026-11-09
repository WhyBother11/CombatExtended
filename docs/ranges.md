# Ranges

Ranges are used in a variety of trigger actions to specify what tiles should be used to apply effect to. Range data is located in 
`Mods/WhyBother11.CombatExtended/Ranges`. This is a dictionary with key being projectile id and value being
data model described below.

## RangeData

| Field   | Type                  | Description                                                                                       |
|---------|-----------------------|---------------------------------------------------------------------------------------------------|
| `Tiles` | `List<RangeTileData>` | Describe adding or removing tiles for this range. Mechanisms and data models are described below. |

### RangeTileData

| Field         | Type                            | Description                                                                                         |
|---------------|---------------------------------|-----------------------------------------------------------------------------------------------------|
| `Id`          | `string`                        | The unique ID of this range builder within the list.                                                |
| `Type`        | `string`                        | `Add` if tiles that match described parameters below should be added to range, `Remove` if removed. |
| `RelativeTo`  | `string`                        | Relative to what tile should this data be calculated. Can be one of `cursor`, `player`, `special`.  |
| `Rectangles`  | `List<RangeTileData_Rectangle>` | *(Optional)* List of rectangles that should be used to calculate tiles.                             |
| `Radius`      | `int`                           | *(Optional)* Radius in tiles that should be used to calculate tiles.                                |
| `Lines`       | `List<RangeTileData_Line>`      | *(Optional)* List of lines that should be used to calculate tiles.                                  |
| `CursorLines` | `List<RangeTileData_CursorLine` | *(Optional)* List of lines aimed at cursor that should be used to calculate tiles.                  |

#### RangeTileData_Rectangle

| Field  | Type         | Description                                                   |
|--------|--------------|---------------------------------------------------------------|
| `Id`   | `string`     | The unique ID of this rectangle within the list.              |
| `From` | `Coordinate` | *(Optional)* The left- and topmost tile of this rectange.     |
| `To`   | `Coordinate` | *(Optional)* The right- and bottommost tile of this rectange. |

#### RangeTileData_Line

| Field       | Type         | Description                                                                |
|-------------|--------------|----------------------------------------------------------------------------|
| `Id`        | `string`     | The unique ID of this line within the list.                                |
| `Angle`     | `int`        | The angle of this line in degrees. Relative to x axis and right direction. |
| `MaxLength` | `int`        | *(Optional)* Length of this line in tiles. Defaults to `30`.               |
| `Width`     | `int`        | *(Optional)* Width of this line in tiles. Defaults to `1`.                 |
| `Delta`     | `Coordinate` | *(Optional)* The delta to apply to this line.                              |

#### RangeTileData_CursorLine

| Field       | Type         | Description                                                  |
|-------------|--------------|--------------------------------------------------------------|
| `Id`        | `string`     | The unique ID of this cursor line within the list.           |
| `MaxLength` | `int`        | *(Optional)* Length of this line in tiles. Defaults to `30`. |
| `Width`     | `int`        | *(Optional)* Width of this line in tiles. Defaults to `1`.   |
| `Delta`     | `Coordinate` | *(Optional)* The delta to apply to this line.                |

#### Coordinate
| Field | Type         | Description                                     |
|-------|--------------|-------------------------------------------------|
| `X`   | `int`        | *(Optional)* The X coordinate. Defaults to `0`. |
| `Y`   | `int`        | *(Optional)* The Y coordinate. Defaults to `0`. |

### Range calculation

Range calculation is applied sequentially based on order in `Tiles`. For every tile in `Tiles`'s `RangeTileData`, if its type is
`Add`, it is added to the result tile set. If type is `Remove`, those tiles are removed from the set, meaning that it takes no effect on
any future operations or if tile was not previously added.

For example, given `Tiles` contain two `RangeTileData`, both being relative to player and one having `Add` type and being circle with radius = 5, and the other being `Remove`
type with one rectangle defined with (-6, -1) and (6, 1). This will result in two parts of circle and no tiles in player's row, in the rows below and
over player tile. If rectangles were first, the result will be just a circle, because no tiles were added yet.

`RelativeTo` describes against which tile range should be calculated. If it is `player`, the range is calculated relative to player tile. If it is
`cursor`, the range is calculated relative to cursor tile. Ranges containing `special` can be used effectively in certain contexts and
represent context dependant tiles. Currently, it can be used in `Monster context`, in which case it assumes context monster tile. 

For example, `RangeTileData` with `RelativeTo` being `cursor` and `RangeTileData` containing rectangle defined with (-2, -2) and (2, 2) will result
in a 3x3 range with center in cursor's current tile.