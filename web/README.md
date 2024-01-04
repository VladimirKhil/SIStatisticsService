SIStatistics service web client.

# Install

    npm install sistatistics-client

# Example usage

```typescript
import SIStatisticsClient, { GamePlatforms } from 'sistatistics-client';

const client = new SIStatisticsClient({ serviceUri: '<insert service address here>' });

const now = new Date();
const ONE_HOUR = 60 * 60 * 1000;

const statistics = await siStatisticsClient.getLatestTopPackagesAsync({
    platform: GamePlatforms.GameServer,
    from: new Date(now.getTime() - ONE_HOUR),
    to: now,
});

console.log(statistics);
```