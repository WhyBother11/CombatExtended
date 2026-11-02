## Contexts

Some game state queries and trigger actions added by the mod can be used only in certain scenarios or may be used differently depending on
whether they were used in certain scenarios. For ease of use and understanding such scenarios are referenced as `contexts` in the documentation.
If added by the framework trigger provides any context, it will be mentioned in documentation.

For example, game state query `MONSTER_HAS_TAG` can be referenced only in a `Monster context`, which provides necessary information to handle
this query.

The existing contexts are:

- `Monster context`;
- `Monster buff context`;
- `Projectile context`;
- `Ability context`;
- `Attack context`.