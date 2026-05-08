# ScanServices Implementation Guide ($safeitemname$)

## Overview

Implements a service for scanning and retrieving elements from the model based on defined criteria. It centralizes scanning logic and provides a consistent interface for element queries across the add-in. ScanServices requires **ScanCriteriaHelpers**.

---

## Dependencies

ScanServices depends on the following ScanCriteriaHelpers.h for scan setup and execution:

```cpp
#include "ScanCriteriaHelpers.h"
```
---